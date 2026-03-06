using System.IO;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 存档槽位路径封装
    /// </summary>
    public class SaveSlot
    {
        public int Index { get; }
        public string SlotDir { get; }
        public string DataPath { get; }
        public string MetaPath { get; }
        public string TempDataPath { get; }

        public SaveSlot(string savesRootDir, int index)
        {
            Index = index;
            SlotDir = Path.Combine(savesRootDir, $"slot_{index}");
            DataPath = Path.Combine(SlotDir, "save.dat");
            MetaPath = Path.Combine(SlotDir, "meta.json");
            TempDataPath = Path.Combine(SlotDir, "save.dat.tmp");
        }

        public bool Exists() => File.Exists(DataPath);
    }
}
