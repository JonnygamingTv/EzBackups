# EzBackups

```xml
<?xml version="1.0" encoding="utf-8"?>
<Config xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <BackupCopies>8</BackupCopies> <!-- Amount of zip-files per BackupFolder -->
  <MinutesBetweenBackups>180</MinutesBetweenBackups> <!-- Set to 0 -->
  <AtBackup>0</AtBackup> <!-- Just plugin keeping track. -->
  <BackupDir>EzBackups/Backups</BackupDir> <!-- Relative from /U3DS/Servers/x/ -->
  <CompressLevel>Optimal</CompressLevel> <!-- CompressionLevel: Optimal, Fastest, NoCompression -->
  <BackupFolders> <!-- Relative from /U3DS/Servers/x/ -->
    <Dir>Level/{0}</Dir> <!-- Replaces {0} with LevelName -->
    <Dir>Players/</Dir>
  </BackupFolders>
</Config>
```
