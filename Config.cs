using Rocket.API;
using System.Collections.Generic;
using System.IO.Compression;
using System.Xml.Serialization;

namespace EzBackups
{
    public class Config : IRocketPluginConfiguration
    {
        public byte BackupCopies;
        public uint MinutesBetweenBackups;
        public uint AtBackup;
        public string BackupDir;
        public CompressionLevel CompressLevel;
        [XmlArrayItem(ElementName = "Dir")]
        public List<string> BackupFolders;
        public void LoadDefaults()
        {
            BackupCopies = 7;
            MinutesBetweenBackups = 30;
            AtBackup = 0;
            BackupDir = "EzBackups/Backups";
            CompressLevel = CompressionLevel.Optimal;
            BackupFolders = new List<string>()
            {
                "Level/{0}", "Players/"
            };
        }
    }
}
