for /f %%i in ('git -C temp rev-parse --abbrev-ref HEAD') do set VAR=%%i
git -C temp diff Start..HEAD > %VAR%.patch
pause