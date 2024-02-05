@echo off

REM convert all to mp3 and move wavs to subfolder

mkdir _mp3

FOR %%f IN (*.wav) DO (
  IF EXIST "_mp3\%%~nf.mp3" (
      ECHO "_mp3\%%~nf.mp3 exists. SKIP..."
  ) ELSE (
      _lame -V2 "%%f" "_mp3\%%~nf.mp3"
  )
)



REM mkdir wav
REM move "%~dp0*.wav" "%~dp0wav"

REM mkdir _mp3
REM move "%~dp0*.mp3" "%~dp0_mp3"

set /p DUMMY=Hit ENTER to continue...