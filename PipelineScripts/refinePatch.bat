@echo off
echo Enter the prefix of the patch file you want to load into the code
set /p PatchFile=
git -C temp checkout master
git -C temp apply ../%PatchFile%.patch
git -C temp checkout -b %PatchFile%
git -C temp add -A
git -C temp commit -m "Loaded patch: %PatchFile%"
pause