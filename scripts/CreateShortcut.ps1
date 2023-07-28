param ( [string]$SourceExe, [string]$WorkingDir, [string]$ArgumentsToSourceExe, [string]$DestinationPath )
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($DestinationPath)
$Shortcut.TargetPath = $SourceExe
$Shortcut.Arguments = $ArgumentsToSourceExe
$Shortcut.WorkingDirectory = $WorkingDir
$Shortcut.Save()