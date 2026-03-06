using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager : Core.ModuleBase
    {
        public override int Priority => 7;

        private AudioSource _musicSource;
        private List<AudioSource> _sfxSources = new List<AudioSource>();
        private int[] _sfxPriorities; // 与 _sfxSources 并行，记录各 Source 当前播放优先级
        private GameObject _audioRoot;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _musicMuted = false;
        private bool _sfxMuted = false;

        private const int MaxSFXSources = 10;

        public override void OnInit()
        {
            _audioRoot = new GameObject("AudioRoot");
            GameObject.DontDestroyOnLoad(_audioRoot);

            // 创建音乐AudioSource
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(_audioRoot.transform);
            _musicSource = musicObj.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            // 创建音效AudioSource池
            for (int i = 0; i < MaxSFXSources; i++)
            {
                GameObject sfxObj = new GameObject($"SFXSource_{i}");
                sfxObj.transform.SetParent(_audioRoot.transform);
                AudioSource sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                _sfxSources.Add(sfxSource);
            }
            _sfxPriorities = new int[MaxSFXSources];
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayMusic(string musicName, bool loop = true)
        {
            AudioClip clip = Core.GameEntry.Resource.Load<AudioClip>($"Audio/Music/{musicName}");
            if (clip != null)
            {
                _musicSource.clip = clip;
                _musicSource.loop = loop;
                _musicSource.volume = _musicMuted ? 0 : _musicVolume;
                _musicSource.Play();
            }
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopMusic()
        {
            _musicSource.Stop();
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseMusic()
        {
            _musicSource.Pause();
        }

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void ResumeMusic()
        {
            _musicSource.UnPause();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="sfxName">音效资源名</param>
        /// <param name="volumeScale">音量缩放</param>
        /// <param name="priority">优先级（越高越不易被抢占，默认 0）</param>
        public void PlaySFX(string sfxName, float volumeScale = 1f, int priority = 0)
        {
            AudioClip clip = Core.GameEntry.Resource.Load<AudioClip>($"Audio/SFX/{sfxName}");
            if (clip != null)
            {
                int sourceIndex = GetAvailableSFXSourceIndex(priority);
                if (sourceIndex >= 0)
                {
                    AudioSource source = _sfxSources[sourceIndex];
                    source.Stop(); // 停止当前音效（空闲 Source 无影响）
                    source.volume = (_sfxMuted ? 0 : _sfxVolume) * volumeScale;
                    source.clip = clip;
                    source.Play();
                    _sfxPriorities[sourceIndex] = priority;
                }
                else
                {
                    Debug.LogWarning($"[AudioManager] All SFX sources are busy with higher or equal priority. Skipping: {sfxName}");
                }
            }
        }

        /// <summary>
        /// 播放3D音效
        /// </summary>
        public void PlaySFX3D(string sfxName, Vector3 position, float volumeScale = 1f)
        {
            AudioClip clip = Core.GameEntry.Resource.Load<AudioClip>($"Audio/SFX/{sfxName}");
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, (_sfxMuted ? 0 : _sfxVolume) * volumeScale);
            }
        }

        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (!_musicMuted)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 静音/取消静音音乐
        /// </summary>
        public void MuteMusic(bool mute)
        {
            _musicMuted = mute;
            _musicSource.volume = mute ? 0 : _musicVolume;
        }

        /// <summary>
        /// 静音/取消静音音效
        /// </summary>
        public void MuteSFX(bool mute)
        {
            _sfxMuted = mute;
        }

        /// <summary>
        /// 获取可用 SFX Source 的索引：优先返回空闲 Source；
        /// 无空闲时返回优先级最低且低于 requestPriority 的 Source；
        /// 否则返回 -1（跳过）。
        /// </summary>
        private int GetAvailableSFXSourceIndex(int requestPriority)
        {
            // 优先找空闲 Source
            for (int i = 0; i < _sfxSources.Count; i++)
            {
                if (!_sfxSources[i].isPlaying)
                    return i;
            }

            // 无空闲时找优先级最低的 Source，若低于请求优先级则抢占
            int lowestPriority = requestPriority;
            int lowestIndex = -1;
            for (int i = 0; i < _sfxSources.Count; i++)
            {
                if (_sfxPriorities[i] < lowestPriority)
                {
                    lowestPriority = _sfxPriorities[i];
                    lowestIndex = i;
                }
            }
            return lowestIndex;
        }

        public override void OnDestroy()
        {
            if (_audioRoot != null)
            {
                GameObject.Destroy(_audioRoot);
            }
            _sfxSources.Clear();
        }
    }
}
