@echo off
echo This Script will take a few minutes to complete

:: Do certain steps only once
IF NOT EXIST temp\ (

:: Install IlSpyCmd
dotnet tool install --global ilspycmd --version 7.1.0.6543

:: Decompile executable
mkdir temp
ilspycmd ../AxiomVerge2.exe -o temp -p

:: Backup original executable
mkdir backup
copy ..\AxiomVerge2.exe backup

:: Restore project
dotnet restore temp

:: Change build directory to original directory for convienience in vs
for /F "tokens=*" %%i in (temp\AxiomVerge2.csproj) do (
if not "%%i" equ "<TargetFramework>net45</TargetFramework>" (echo %%i)
if "%%i" equ "<AssemblyName>AxiomVerge2</AssemblyName>" (echo ^<OutDir^>../../^</OutDir^> & echo ^<TargetFramework^>net481^</TargetFramework^>)) >> temp.txt
move /y temp.txt temp\AxiomVerge2.csproj

:: Unzip the EmbeddedContent Files
cd temp/OuterBeyond
mkdir EmbeddedContent.Content
cd EmbeddedContent.Content
tar -xf ../EmbeddedContent.Content.zip
cd ../../../

:: Initialize Repository for patches
git init temp
(echo /bin/ & echo /obj/ & echo /.vs/ & echo /AxiomVerge2.sln & echo /AxiomVerge2.csproj.user & echo /OuterBeyond/EmbeddedContent.Content.zip) > temp\.gitignore
git -C temp add -A
git -C temp commit -m "Initialized Repo"
git -C temp tag "Untouched" HEAD

:: git -C temp apply ../start.diff 
:: git -C temp add -A
:: git -C temp commit -m "Compileable Repo"
git -C temp tag "Start" HEAD)

echo Finished
pause