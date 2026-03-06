namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 存档系统配置
    /// </summary>
    [System.Serializable]
    public class SaveConfig
    {
        /// <summary>
        /// 最大槽位数量
        /// </summary>
        public int MaxSlots = 3;

        /// <summary>
        /// 是否启用 AES 加密
        /// </summary>
        public bool EnableEncryption = false;

        /// <summary>
        /// AES 加密密钥（启用加密时必须设置）
        /// </summary>
        public string EncryptionKey = "";

        /// <summary>
        /// 是否启用 GZIP 压缩
        /// </summary>
        public bool EnableCompression = false;

        /// <summary>
        /// 当前存档数据版本号
        /// </summary>
        public int CurrentVersion = 1;

        /// <summary>
        /// 存档根目录名称（相对于 persistentDataPath）
        /// </summary>
        public string SaveDirName = "Saves";
    }
}
