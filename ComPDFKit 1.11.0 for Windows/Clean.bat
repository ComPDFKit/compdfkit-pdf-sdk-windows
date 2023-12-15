@echo off
setlocal

set "targetFolder=%~dp0"

call :DeleteFolders ".\Examples"
call :DeleteFolders ".\Output"
call :DeleteFolders ".\.vs"  

echo Cleanup complete!
pause
exit

:DeleteFolders
for /d %%i in ("%1*") do (
    if /i "%%~nxi"=="obj" (
        echo Deleting folder "%%i"
        rd /s /q "%%i"
    ) else (
        if /i "%%~nxi"=="bin" (
            echo Deleting folder "%%i"
            rd /s /q "%%i"
        ) else (
            if /i "%%~nxi"=="Output" (
                echo Deleting folder "%%i"
                rd /s /q "%%i"
            ) else (
                if /i "%%~nxi"==".vs" (  
                    echo Deleting folder "%%i"
                    rd /s /q "%%i"
                ) else (
                    call :DeleteFolders "%%i\"
                )
            )
        )
    )
)
exit /b
