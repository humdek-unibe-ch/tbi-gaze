@ECHO off
SETLOCAL

SET source=%CD%\GazeControl.exe

CALL :CreateShortcut CUSTOM_CALIBRATE , CustomCalibrate
CALL :CreateShortcut DRIFT_COMPENSATION , DriftCompensation
CALL :CreateShortcut TERMINATE , GazeClose
CALL :CreateShortcut GAZE_RECORDING_DISABLE , GazeRecordingDisable
CALL :CreateShortcut GAZE_RECORDING_ENABLE , GazeRecordingEnable
CALL :CreateShortcut MOUSE_TRACKING_DISABLE , MouseTrackingDisable
CALL :CreateShortcut MOUSE_TRACKING_ENABLE , MouseTrackingEnable
CALL :CreateShortcut RESET_DRIFT_COMPENSATION , ResetDriftCompensation
CALL :CreateShortcut RESET_START_TIME , ResetStartTime

EXIT /B %ERRORLEVEL%

:CreateShortcut
DEL ".\%~2.lnk"
POWERSHELL -ExecutionPolicy unrestricted -File .\CreateShortcut.ps1 "%source%" "%CD%" "/command %~1" ".\%~2.lnk"
EXIT /B 0