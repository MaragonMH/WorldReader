@echo off
set /p filename="Enter File to decode without extension: "
openssl enc -aes-128-cbc -d -in %filename%.sav -out Decoded%filename%.sav -K "baadf00d00000000203044c213e41fff" -iv "e5ffffffe5ba0700baadf00dff00ff00" && echo Sucess
pause