@echo off
echo This Script will take a few minutes to complete

:: Do certain steps only once
IF NOT EXIST temp\ (
:: Decompile executable
mkdir temp
ilspycmd ../AxiomVerge2.exe -o temp -p

:: Backup original executable
copy ..\AxiomVerge2.exe backup

:: Restore project
dotnet restore temp

:: Change build directory to original directory for convienience in vs
for /F "tokens=*" %%i in (temp\AxiomVerge2.csproj) do (echo %%i 
if "%%i" equ "<AssemblyName>AxiomVerge2</AssemblyName>" (echo ^<OutDir^>../../^</OutDir^>)) >> temp.txt
move /y temp.txt temp\AxiomVerge2.csproj

:: Initialize Repository for patches
git init temp
(echo /bin/ & echo /obj/Debug/ & echo /.vs/) > temp\.gitignore
git -C temp add -A
git -C temp commit -m "Initialized Repo"
git -C temp tag "Untouched" HEAD
git -C temp apply ../start.diff 
git -C temp add -A
git -C temp commit -m "Compileable Repo"
git -C temp tag "Start" HEAD)

:: Apply patches
git -C temp checkout master
for /f %%i in ('dir /b *.patch') do git -C temp apply --3way --ignore-space-change ../%%i

:: Build source
MSBuild.exe temp/AxiomVerge2.csproj -p:OutDir=bin

:: Replace original
copy temp\bin\AxiomVerge2.exe ..\AxiomVerge2.exe

echo Finished
pause