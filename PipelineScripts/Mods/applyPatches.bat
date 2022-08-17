:: Apply patches
git -C temp checkout master
for /f %%i in ('dir /b *.patch') do git -C temp apply --3way --ignore-space-change ../%%i

:: Build source
dotnet build temp

@echo off
echo Finished. Check for errors above. Warnings are ok
pause