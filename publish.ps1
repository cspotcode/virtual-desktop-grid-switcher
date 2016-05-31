rm -r publish
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