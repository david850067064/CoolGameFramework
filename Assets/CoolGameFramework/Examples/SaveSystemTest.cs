using System.Collections;
using UnityEngine;
using CoolGameFramework.Core;
using CoolGameFramework.Modules;

namespace CoolGameFramework.Examples
{
    /// <summary>
    /// SaveManager 手动测试脚本
    /// 挂载到场景中的 GameObject，运行后在 Console 查看结果
    /// </summary>
    public class SaveSystemTest : MonoBehaviour
    {
        [System.Serializable]
        private class PlayerData
        {
            public string PlayerName;
            public int Level;
            public float Health;
            public int Gold;
        }

        private void Start()
        {
            StartCoroutine(RunAllTests());
        }

        private IEnumerator RunAllTests()
        {
            Debug.Log("===== SaveSystem Tests Begin =====");

            Test_BasicSaveLoad();
            yield return null;

            Test_MultiSlot();
            yield return null;

            Test_DeleteSlot();
            yield return null;

            Test_MetaData();
            yield return null;

            Test_EncryptionMissingKey();
            yield return null;

            Test_LoadMissingSlot();
            yield return null;

            yield return StartCoroutine(Test_VersionMigration());

            yield return StartCoroutine(Test_VersionMigrationMissing());

            yield return StartCoroutine(Test_AsyncSaveLoad());

            Debug.Log("===== SaveSystem Tests End =====");
        }

        // ─── 7.1 基础存取、多槽位、删除、元数据 ──────────────────────

        private void Test_BasicSaveLoad()
        {
            Debug.Log("[Test] Basic Save & Load");

            var data = new PlayerData { PlayerName = "Hero", Level = 5, Health = 100f, Gold = 500 };
            GameEntry.Save.Save(0, data);

            var loaded = GameEntry.Save.Load<PlayerData>(0);
            Debug.Assert(loaded != null, "Loaded data should not be null");
            Debug.Assert(loaded.PlayerName == "Hero", $"Expected 'Hero', got '{loaded.PlayerName}'");
            Debug.Assert(loaded.Level == 5, $"Expected Level 5, got {loaded.Level}");
            Debug.Log("[Test] Basic Save & Load PASSED");
        }

        private void Test_MultiSlot()
        {
            Debug.Log("[Test] Multi-Slot Save & Load");

            var slot0Data = new PlayerData { PlayerName = "Slot0", Level = 1, Gold = 100 };
            var slot1Data = new PlayerData { PlayerName = "Slot1", Level = 99, Gold = 9999 };

            GameEntry.Save.Save(0, slot0Data);
            GameEntry.Save.Save(1, slot1Data);

            var loaded0 = GameEntry.Save.Load<PlayerData>(0);
            var loaded1 = GameEntry.Save.Load<PlayerData>(1);

            Debug.Assert(loaded0.PlayerName == "Slot0", "Slot 0 data mismatch");
            Debug.Assert(loaded1.PlayerName == "Slot1", "Slot 1 data mismatch");
            Debug.Assert(loaded0.Gold != loaded1.Gold, "Slots should be independent");
            Debug.Log("[Test] Multi-Slot PASSED");
        }

        private void Test_DeleteSlot()
        {
            Debug.Log("[Test] Delete Slot");

            var data = new PlayerData { PlayerName = "ToDelete", Level = 1 };
            GameEntry.Save.Save(2, data);
            Debug.Assert(GameEntry.Save.CanLoad(2), "Slot 2 should exist before delete");

            GameEntry.Save.DeleteSlot(2);
            Debug.Assert(!GameEntry.Save.CanLoad(2), "Slot 2 should not exist after delete");
            Debug.Log("[Test] Delete Slot PASSED");
        }

        private void Test_MetaData()
        {
            Debug.Log("[Test] Metadata Read");

            GameEntry.Save.Save(0, new PlayerData { PlayerName = "MetaTest" });
            var meta = GameEntry.Save.GetSlotMeta(0);

            Debug.Assert(meta != null, "Meta should not be null");
            Debug.Assert(meta.SlotIndex == 0, "Meta SlotIndex mismatch");
            Debug.Assert(meta.LastSaveTime > 0, "Meta LastSaveTime should be set");
            Debug.Assert(meta.DataVersion == 1, $"Expected version 1, got {meta.DataVersion}");

            var allMetas = GameEntry.Save.GetAllSlotMetas();
            Debug.Assert(allMetas.Length == 3, $"Expected 3 slots, got {allMetas.Length}");
            Debug.Assert(allMetas[0] != null, "Slot 0 meta should exist");

            Debug.Log("[Test] Metadata PASSED");
        }

        // ─── 7.3 异常路径 ─────────────────────────────────────────────

        private void Test_EncryptionMissingKey()
        {
            Debug.Log("[Test] Encryption Missing Key");

            var cfg = new SaveConfig
            {
                MaxSlots = 3,
                EnableEncryption = true,
                EncryptionKey = "",   // 故意留空
                CurrentVersion = 1
            };

            // 临时配置（测试完恢复默认）
            GameEntry.Save.Configure(cfg);

            bool threw = false;
            try
            {
                GameEntry.Save.Save(0, new PlayerData { PlayerName = "EncTest" });
            }
            catch (System.InvalidOperationException)
            {
                threw = true;
            }
            Debug.Assert(threw, "Should throw InvalidOperationException when key is empty");

            // 恢复默认配置
            GameEntry.Save.Configure(new SaveConfig());
            Debug.Log("[Test] Encryption Missing Key PASSED");
        }

        private void Test_LoadMissingSlot()
        {
            Debug.Log("[Test] Load Missing Slot");

            GameEntry.Save.DeleteSlot(2); // 确保不存在
            var result = GameEntry.Save.Load<PlayerData>(2);
            Debug.Assert(result == null, "Loading missing slot should return null");
            Debug.Log("[Test] Load Missing Slot PASSED");
        }

        // ─── 7.2 版本迁移 ─────────────────────────────────────────────

        private IEnumerator Test_VersionMigration()
        {
            Debug.Log("[Test] Version Migration");

            // 用 v1 配置保存一条旧数据
            var cfgV1 = new SaveConfig { MaxSlots = 3, CurrentVersion = 1 };
            GameEntry.Save.Configure(cfgV1);
            GameEntry.Save.Save(2, new PlayerData { PlayerName = "OldHero", Level = 10 });

            // 切换到 v2，注册 v1→v2 迁移回调（模拟添加新字段默认值）
            var cfgV2 = new SaveConfig { MaxSlots = 3, CurrentVersion = 2 };
            GameEntry.Save.Configure(cfgV2);
            GameEntry.Save.RegisterMigration(1, 2, json =>
            {
                // 简单追加字段（JsonUtility 不支持 JObject，此处模拟字符串替换）
                return json.Replace("}", ", \"Gold\": 9999}");
            });

            yield return null;

            var loaded = GameEntry.Save.Load<PlayerData>(2);
            Debug.Assert(loaded != null, "Migrated data should load successfully");
            Debug.Log($"[Test] Version Migration PASSED. PlayerName={loaded.PlayerName}");

            // 恢复默认
            GameEntry.Save.Configure(new SaveConfig());
        }

        private IEnumerator Test_VersionMigrationMissing()
        {
            Debug.Log("[Test] Version Migration Missing Callback");

            var cfgV1 = new SaveConfig { MaxSlots = 3, CurrentVersion = 1 };
            GameEntry.Save.Configure(cfgV1);
            GameEntry.Save.Save(2, new PlayerData { PlayerName = "OldData" });

            // 切换到 v3 但不注册任何迁移
            var cfgV3 = new SaveConfig { MaxSlots = 3, CurrentVersion = 3 };
            GameEntry.Save.Configure(cfgV3);

            yield return null;

            bool threw = false;
            try
            {
                GameEntry.Save.Load<PlayerData>(2);
            }
            catch (SaveVersionException)
            {
                threw = true;
            }
            Debug.Assert(threw, "Should throw SaveVersionException when migration is missing");
            Debug.Log("[Test] Version Migration Missing PASSED");

            GameEntry.Save.Configure(new SaveConfig());
        }

        // ─── 7.1 异步存取 ─────────────────────────────────────────────

        private IEnumerator Test_AsyncSaveLoad()
        {
            Debug.Log("[Test] Async Save & Load");

            var data = new PlayerData { PlayerName = "AsyncHero", Level = 42, Gold = 888 };
            var saveTask = GameEntry.Save.SaveAsync(0, data);

            while (!saveTask.IsCompleted)
                yield return null;

            Debug.Assert(!saveTask.IsFaulted, $"SaveAsync failed: {saveTask.Exception}");

            var loadTask = GameEntry.Save.LoadAsync<PlayerData>(0);

            while (!loadTask.IsCompleted)
                yield return null;

            Debug.Assert(!loadTask.IsFaulted, $"LoadAsync failed: {loadTask.Exception}");
            Debug.Assert(loadTask.Result != null, "Async loaded data should not be null");
            Debug.Assert(loadTask.Result.PlayerName == "AsyncHero", "Async data mismatch");
            Debug.Log("[Test] Async Save & Load PASSED");
        }
    }
}
