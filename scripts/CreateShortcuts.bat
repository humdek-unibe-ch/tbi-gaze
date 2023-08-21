@ECHO off
SETLOCAL

SET source=%CD%\GazeControl.exe

CALL :CreateCmd CUSTOM_CALIBRATE , CustomCalibrate
CALL :CreateCmd DRIFT_COMPENSATION , DriftCompensation
CALL :CreateCmd TERMINATE , GazeClose
CALL :CreateCmd GAZE_RECORDING_DISABLE , GazeRecordingDisable
CALL :CreateCmd GAZE_RECORDING_ENABLE , GazeRecordingEnable
CALL :CreateCmd MOUSE_TRACKING_DISABLE , MouseTrackingDisable
CALL :CreateCmd MOUSE_TRACKING_ENABLE , MouseTrackingEnable
CALL :CreateCmd RESET_DRIFT_COMPENSATION , ResetDriftCompensation
CALL :CreateCmd RESET_START_TIME , ResetStartTime
CALL :CreateCmd VALIDATE , Validate

EXIT /B %ERRORLEVEL%

:CreateShortcut
DEL ".\%~2.lnk"
POWERSHELL -ExecutionPolicy unrestricted -File .\CreateShortcut.ps1 "%source%" "%CD%" "/command %~1" ".\%~2.lnk"
EXIT /B 0

:CreateCmd
DEL ".\%~2.cmd"
ECHO START %source% /command %~1 > "%~2.cmd"
EXIT /B 0