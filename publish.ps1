 param (
    [string]$version = "TODO"
 )

$ErrorActionPreference = "Stop"
$out = "publish"
if(Test-Path $out) {
    rm -r $out
}
mkdir $out
#cp -Recurse Package/* "$out/"
#cp -Recurse bin/Release/* "$out/"
#rm "$out/*.pdb"
#rm -r "$out/app.publish"
#rm "$out/VirtualDesktop.xml"
#rm "$out/*.application"
#rm "$out/*.exe.config"
#rm "$out/*.exe.manifest"
#rm "$out/*.vshost.*"
cp bin/Release/InjectDll32.exe "$out/"
cp bin/Release/InjectDll64.exe "$out/"
cp bin/Release/VDMHelper32.dll "$out/"
cp bin/Release/VDMHelper64.dll "$out/"
cp bin/Release/VDMHelperCLR.Common.dll "$out/"
cp bin/Release/VDMHelperCLR32.dll "$out/"
cp bin/Release/VDMHelperCLR64.dll "$out/"
cp bin/Release/VirtualDesktopGridSwitcher.exe "$out/"
cp bin/Release/VirtualDesktop.dll "$out/"
cp bin/Release/VirtualDesktopGridSwitcher.exe "$out/"
cp -Recurse autohotkey "$out/"
cp -Recurse Icons "$out/"
cp LICENSE.txt "$out/"
cp README.md "$out/"
cp CREDITS.txt "$out/"

Add-Type -assembly "system.io.compression.filesystem"
$dirname = (Resolve-Path "./").ToString()
$source = $dirname + "/$out"
$destination = $dirname + "/VirtualDesktopGridSwitcher-v" + $version + ".zip"
if(Test-Path $destination) {
    rm $destination
}
[io.compression.zipfile]::CreateFromDirectory($source, $destination)
