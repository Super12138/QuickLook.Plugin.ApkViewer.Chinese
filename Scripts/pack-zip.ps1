Remove-Item ..\QuickLook.Plugin.ApkViewer.Chinese.qlplugin -ErrorAction SilentlyContinue

$files = Get-ChildItem -Path ..\release\ -Exclude *.pdb,*.xml
$outputPlugin = '..\..\QuickLook.Plugin.ApkViewer.Chinese.qlplugin'

Compress-Archive $files ..\QuickLook.Plugin.ApkViewer.Chinese.zip -Force
Move-Item ..\QuickLook.Plugin.ApkViewer.Chinese.zip $outputPlugin -Force

[string]::Concat("插件已打包 -> ", [System.IO.Path]::GetFullPath($outputPlugin))