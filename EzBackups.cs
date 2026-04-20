using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.IO;
using System.IO.Compression;

namespace EzBackups
{
    public class EzBackups : RocketPlugin<Config>
    {
        public static EzBackups Instance;
        public bool InitializedHarmony;
        public string LevelName = "";
        public string ServerPath = "";
        private string PluginDir = "";
        public string toZip = "";
        public static System.Threading.Timer _timer;
        protected override void Load()
        {
            Instance = this;
            PluginDir = Rocket.Core.Environment.PluginsDirectory; // System.Reflection.Assembly.GetExecutingAssembly().Location;
            // PluginDir = PluginDir.Substring(0, PluginDir.Length - 4);
            ServerPath = Path.GetFullPath(Path.Combine(PluginDir, "..", "..")); // Path.GetFullPath(Path.Combine(PluginDir, "..", "..", ".."));
            PluginDir = Path.Combine(PluginDir, Configuration.Instance.BackupDir);
            try
            {
                System.IO.Directory.CreateDirectory(PluginDir);
            }
            catch (Exception) { }
            if (Level.isLoaded)
            {
                InitBackups(1);
            }
            else
            {
                Level.onLevelLoaded += InitBackups;
                if(Configuration.Instance.CommandsAfterLoad.Count != 0)
                    Level.onPostLevelLoaded += DoAfterLoad;
            }
            Logger.Log("Serverpath:" + ServerPath);
            Logger.Log("Backups go to:" + Path.GetFullPath(PluginDir));
            if (Configuration.Instance.MinutesBetweenBackups == 0)
            {
                // SDG.Unturned.SaveManager.onPostSave += ;
                try
                {
                    if (InitializedHarmony)
                        return;
                    var harmony = new HarmonyLib.Harmony("EzBackups");
                    // harmony.PatchAll();

                    var processor = new HarmonyLib.PatchClassProcessor(harmony, typeof(InternalPatches));
                    processor.Patch();
                    InitializedHarmony = true;
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "[ERROR] LoadHarmony");
                }
                Logger.Log("Backing up after every Level.save. P.S. You must restart your server if you wish to change this setting.");
            }else
            {
                Logger.Log("Backing up every "+ Configuration.Instance.MinutesBetweenBackups.ToString() + " minute.");
            }
            Provider.onServerShutdown += SaveConf;
        }

        private void DoAfterLoad(int level)
        {
            foreach (string cmd in Configuration.Instance.CommandsAfterLoad)
            {
                Rocket.Core.R.Commands.Execute(null, cmd);
            }
        }

        protected override void Unload()
        {
            Level.onLevelLoaded -= InitBackups;
            Level.onPostLevelLoaded -= DoAfterLoad;
            Provider.onServerShutdown -= SaveConf;
            _timer?.Dispose();
            _timer = null;
        }
        public void SaveConf()
        {
            Configuration.Save();
        }

        private void InitBackups(int level)
        {
            LevelName = SDG.Unturned.Level.info.name;
            Logger.Log("Running timer. Map: "+LevelName);
            if(Configuration.Instance.MinutesBetweenBackups != 0)
                _timer = new System.Threading.Timer(_ => BackupLevel(), null, TimeSpan.FromMinutes(Configuration.Instance.MinutesBetweenBackups), TimeSpan.FromMinutes(Configuration.Instance.MinutesBetweenBackups));
        }
        public void BackupLevel()
        {
            Configuration.Instance.AtBackup = (Configuration.Instance.AtBackup % Configuration.Instance.BackupCopies) + 1;
            foreach(string ToBackup in Configuration.Instance.BackupFolders)
            {
                string ToName = ToBackup.Replace("{0}", LevelName);
                string Res = Path.Combine(ServerPath, ToName);
                ToName = ToName.Replace(Path.DirectorySeparatorChar, '_') + "." + Configuration.Instance.AtBackup.ToString() + ".zip";
#if DEBUG
                Logger.Log("Zipping: "+Res+" | to: " + Path.Combine(PluginDir, ToName));
#endif
                string zipPath = Path.Combine(PluginDir, ToName);
                string tempPath = zipPath + ".tmp";

                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);

                    ZipFile.CreateFromDirectory(
                        Res,
                        tempPath,
                        Configuration.Instance.CompressLevel,
                        includeBaseDirectory: false
                    );

                    if (File.Exists(zipPath))
                        File.Delete(zipPath);

                    File.Move(tempPath, zipPath);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Backup failed");
                }

            }
        }
    }
    [HarmonyPatch]
    internal static class InternalPatches
    {
        [HarmonyPatch(typeof(Level), "save")]
        [HarmonyPostfix]
        internal static void OnLevelSave()
        {
            Rocket.Core.Utils.TaskDispatcher.OffThread(() =>
            {
                EzBackups.Instance?.BackupLevel();
            });
        }
    }
}
