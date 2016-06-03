 param (
    [string]$version = "TODO"
 )

$ErrorActionPreference = "Stop"
if(Test-Path publish) {
    rm -r publish
}
mkdir publish
cp -Recurse Package/* publish/
# cp -Recurse bin/Release/* publish/
# rm publish/*.pdb
# rm -r publish/app.publish
# rm publish/VirtualDesktop.xml
# rm publish/*.application
# rm publish/*.exe.config
# rm publish/*.exe.manifest
# rm publish/*.vshost.*
cp bin/Release/VirtualDesktopGridSwitcher.exe publish/
cp bin/Release/VirtualDesktop.dll publish/
cp -Recurse autohotkey publish/
cp LICENSE.txt publish/
cp README.md publish/
cp CREDITS.txt publish/

Add-Type -assembly "system.io.compression.filesystem"
$dirname = (Resolve-Path "./").ToString()
$source = $dirname + "/publish"
$destination = $dirname + "/VirtualDesktopGridSwitcher-v" + $version + ".zip"
if(Test-Path $destination) {
    rm $destination
}
[io.compression.zipfile]::CreateFromDirectory($source, $destination)
