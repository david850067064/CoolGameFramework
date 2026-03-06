using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CoolGameFramework.Utilities;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 存档版本不兼容异常
    /// </summary>
    public class SaveVersionException : Exception
    {
        public int FromVersion { get; }
        public int ToVersion { get; }

        public SaveVersionException(int fromVersion, int toVersion)
            : base($"Save data version mismatch: found v{fromVersion}, expected v{toVersion}. No migration registered for this path.")
        {
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }
    }

    /// <summary>
    /// 存档管理器
    /// 通过 GameEntry.Save 访问
    /// </summary>
    public class SaveManager : Core.ModuleBase
    {
        public override int Priority => 4;

        private SaveConfig _config;
        private string _savesRootDir;
        private readonly Dictionary<(int, int), Func<string, string>> _migrations
            = new Dictionary<(int, int), Func<string, string>>();
        private readonly List<Task> _pendingWrites = new List<Task>();

        // ─── 生命周期 ─────────────────────────────────────────────

        public override void OnInit()
        {
            _config = new SaveConfig();
            _savesRootDir = Path.Combine(Application.persistentDataPath, _config.SaveDirName);
            FileUtil.CreateDirectory(_savesRootDir);
            Debug.Log($"[SaveManager] Initialized. Save dir: {_savesRootDir}");
        }

        /// <summary>
        /// 覆盖默认配置（在 OnInit 之后、第一次 Save 之前调用）
        /// </summary>
        public void Configure(SaveConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _savesRootDir = Path.Combine(Application.persistentDataPath, _config.SaveDirName);
            FileUtil.CreateDirectory(_savesRootDir);
        }

        public override void OnDestroy()
        {
            // 等待所有未完成的异步写入
            if (_pendingWrites.Count > 0)
            {
                Task.WaitAll(_pendingWrites.ToArray());
                _pendingWrites.Clear();
            }
        }

        // ─── 同步写入 ──────────────────────────────────────────────

        /// <summary>
        /// 同步保存存档数据到指定槽位
        /// </summary>
        public void Save<T>(int slotIndex, T data)
        {
            ValidateSlotIndex(slotIndex);
            ValidateEncryptionConfig();

            var slot = new SaveSlot(_savesRootDir, slotIndex);
            FileUtil.CreateDirectory(slot.SlotDir);

            string json = JsonUtil.ToJson(data);
            string content = ApplyCompressionAndEncryption(json);

            // 原子写入：先写 .tmp 再替换
            File.WriteAllText(slot.TempDataPath, content, Encoding.UTF8);
            if (File.Exists(slot.DataPath))
                File.Delete(slot.DataPath);
            File.Move(slot.TempDataPath, slot.DataPath);

            WriteMeta(slot, slotIndex);
        }

        // ─── 同步读取 ──────────────────────────────────────────────

        /// <summary>
        /// 从指定槽位同步加载存档数据，槽位不存在时返回 default
        /// </summary>
        public T Load<T>(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);

            var slot = new SaveSlot(_savesRootDir, slotIndex);
            if (!slot.Exists())
                return default;

            var meta = ReadMetaInternal(slot);
            string content = File.ReadAllText(slot.DataPath, Encoding.UTF8);
            string json = ReverseCompressionAndEncryption(content);

            json = ApplyMigrations(json, meta?.DataVersion ?? _config.CurrentVersion);

            return JsonUtil.FromJson<T>(json);
        }

        // ─── 删除 ──────────────────────────────────────────────────

        /// <summary>
        /// 删除指定槽位的所有存档文件
        /// </summary>
        public void DeleteSlot(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            var slot = new SaveSlot(_savesRootDir, slotIndex);
            FileUtil.DeleteDirectory(slot.SlotDir);
            Debug.Log($"[SaveManager] Slot {slotIndex} deleted.");
        }

        // ─── 元数据查询 ────────────────────────────────────────────

        /// <summary>
        /// 获取指定槽位的元数据（不存在时返回 null）
        /// </summary>
        public SaveMeta GetSlotMeta(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            var slot = new SaveSlot(_savesRootDir, slotIndex);
            return ReadMetaInternal(slot);
        }

        /// <summary>
        /// 获取所有槽位的元数据，空槽位以 null 占位
        /// </summary>
        public SaveMeta[] GetAllSlotMetas()
        {
            var metas = new SaveMeta[_config.MaxSlots];
            for (int i = 0; i < _config.MaxSlots; i++)
            {
                metas[i] = GetSlotMeta(i);
            }
            return metas;
        }

        /// <summary>
        /// 检查指定槽位是否存在且版本兼容
        /// </summary>
        public bool CanLoad(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            var slot = new SaveSlot(_savesRootDir, slotIndex);
            if (!slot.Exists())
                return false;

            var meta = ReadMetaInternal(slot);
            if (meta == null)
                return true; // 无元数据，视为兼容（旧存档）

            int savedVersion = meta.DataVersion;
            if (savedVersion == _config.CurrentVersion)
                return true;

            // 检查是否有完整的迁移路径
            return HasMigrationPath(savedVersion, _config.CurrentVersion);
        }

        // ─── 游戏时长 ──────────────────────────────────────────────

        /// <summary>
        /// 累加指定槽位的游戏时长（秒），供业务层每帧或定期调用
        /// </summary>
        public void UpdatePlayTime(int slotIndex, float deltaSeconds)
        {
            ValidateSlotIndex(slotIndex);
            var slot = new SaveSlot(_savesRootDir, slotIndex);
            var meta = ReadMetaInternal(slot);
            if (meta == null)
                return;

            meta.PlayTime += deltaSeconds;
            WriteMetaDirect(slot, meta);
        }

        // ─── 异步 API ──────────────────────────────────────────────

        /// <summary>
        /// 异步保存存档数据到指定槽位
        /// </summary>
        public Task SaveAsync<T>(int slotIndex, T data)
        {
            var task = Task.Run(() => Save(slotIndex, data));
            _pendingWrites.Add(task);
            task.ContinueWith(t => _pendingWrites.Remove(t));
            return task;
        }

        /// <summary>
        /// 从指定槽位异步加载存档数据
        /// </summary>
        public Task<T> LoadAsync<T>(int slotIndex)
        {
            return Task.Run(() => Load<T>(slotIndex));
        }

        // ─── 版本迁移 ──────────────────────────────────────────────

        /// <summary>
        /// 注册版本迁移回调。migrateFunc 接收原始 JSON 字符串，返回迁移后的 JSON 字符串。
        /// </summary>
        public void RegisterMigration(int fromVersion, int toVersion, Func<string, string> migrateFunc)
        {
            if (migrateFunc == null)
                throw new ArgumentNullException(nameof(migrateFunc));

            _migrations[(fromVersion, toVersion)] = migrateFunc;
        }

        // ─── 私有方法 ──────────────────────────────────────────────

        private string ApplyCompressionAndEncryption(string json)
        {
            string content = json;

            if (_config.EnableCompression)
                content = GZipCompress(content);

            if (_config.EnableEncryption)
                content = EncryptUtil.AESEncrypt(content, _config.EncryptionKey);

            return content;
        }

        private string ReverseCompressionAndEncryption(string content)
        {
            string result = content;

            if (_config.EnableEncryption)
                result = EncryptUtil.AESDecrypt(result, _config.EncryptionKey);

            if (_config.EnableCompression)
                result = GZipDecompress(result);

            return result;
        }

        private string ApplyMigrations(string json, int savedVersion)
        {
            if (savedVersion == _config.CurrentVersion)
                return json;

            int current = savedVersion;
            while (current != _config.CurrentVersion)
            {
                int next = current + 1; // 逐步迁移
                if (!_migrations.TryGetValue((current, next), out var migrate))
                    throw new SaveVersionException(savedVersion, _config.CurrentVersion);

                json = migrate(json);
                current = next;
            }
            return json;
        }

        private bool HasMigrationPath(int from, int to)
        {
            int current = from;
            while (current != to)
            {
                if (!_migrations.ContainsKey((current, current + 1)))
                    return false;
                current++;
            }
            return true;
        }

        private void WriteMeta(SaveSlot slot, int slotIndex)
        {
            var meta = ReadMetaInternal(slot) ?? new SaveMeta
            {
                SlotIndex = slotIndex,
                CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                PlayTime = 0f
            };

            meta.LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            meta.DataVersion = _config.CurrentVersion;
            WriteMetaDirect(slot, meta);
        }

        private void WriteMetaDirect(SaveSlot slot, SaveMeta meta)
        {
            string metaJson = JsonUtil.ToJson(meta, true);
            File.WriteAllText(slot.MetaPath, metaJson, Encoding.UTF8);
        }

        private SaveMeta ReadMetaInternal(SaveSlot slot)
        {
            if (!File.Exists(slot.MetaPath))
                return null;

            string metaJson = File.ReadAllText(slot.MetaPath, Encoding.UTF8);
            if (string.IsNullOrEmpty(metaJson))
                return null;

            return JsonUtil.FromJson<SaveMeta>(metaJson);
        }

        private void ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.MaxSlots)
                throw new ArgumentOutOfRangeException(nameof(slotIndex),
                    $"Slot index {slotIndex} is out of range [0, {_config.MaxSlots - 1}].");
        }

        private void ValidateEncryptionConfig()
        {
            if (_config.EnableEncryption && string.IsNullOrEmpty(_config.EncryptionKey))
                throw new InvalidOperationException(
                    "[SaveManager] Encryption is enabled but EncryptionKey is not set in SaveConfig.");
        }

        // ─── GZIP 压缩/解压 ────────────────────────────────────────

        private static string GZipCompress(string text)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            using (var outputStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzip.Write(inputBytes, 0, inputBytes.Length);
                }
                return Convert.ToBase64String(outputStream.ToArray());
            }
        }

        private static string GZipDecompress(string compressedBase64)
        {
            byte[] inputBytes = Convert.FromBase64String(compressedBase64);
            using (var inputStream = new MemoryStream(inputBytes))
            using (var gzip = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzip.CopyTo(outputStream);
                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }
    }
}
