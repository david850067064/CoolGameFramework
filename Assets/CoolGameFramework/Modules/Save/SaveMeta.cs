namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 存档槽位元数据（明文存储，无需解密即可读取）
    /// </summary>
    [System.Serializable]
    public class SaveMeta
    {
        /// <summary>
        /// 槽位索引
        /// </summary>
        public int SlotIndex;

        /// <summary>
        /// 首次创建时间（Unix 时间戳，秒）
        /// </summary>
        public long CreateTime;

        /// <summary>
        /// 最后保存时间（Unix 时间戳，秒）
        /// </summary>
        public long LastSaveTime;

        /// <summary>
        /// 累计游戏时长（秒）
        /// </summary>
        public float PlayTime;

        /// <summary>
        /// 存档数据版本号
        /// </summary>
        public int DataVersion;
    }
}
