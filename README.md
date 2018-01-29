# Toolset to control Tobii Eye Tracker 4c

This repository contains the source code for multiple simple tools that allow to control a Tobii eye tracker from a 3rd party application.
Specifically, this project aims at providing a set of executables that can be called from within [ztree](http://www.ztree.uzh.ch/en.html) to allow eye tracker support for economic experiments.

## Installation
In order to run the executables the following files need to be placed in the same directory as the executables:

 - GazeHelper.dll
 - Newtonsoft.Json.dll
 - Tobii.EyeX.Client.dll
 - Tobii.Interaction.Model.dll
 - Tobii.Interaction.Net.dll
 - blank.cur
 - config.json

 The complete package can be downloaded from [here](http://tpf.fluido.as:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/release/v0.1.0.zip).

Further, the Tobii engine must be running and the eye tracker must be enabled.
To install the Tobii engine for the [Tobii Eye Tracker 4c](https://tobiigaming.com/eye-tracker-4c/) download the software from [here](https://tobiigaming.com/downloadlatest/?bundle=tobii-core).

The Tobii engine starts the following service

    Tobii.Service

and the following processes

    Tobii EyeX Engine
    Tobii.EyeX.Interaction.exe
    Tobii.EyeX.Tray.exe

## Executables
The following executables have been prepared:

### TobiiCalibrate.exe
This program is a simple wrapper for the Tobii calibration tool.
It launches the calibration GUI where the user is led through the calibration process and stores the calibration data in the current profile of the eye tracker engine.

### TobiiGuestClaibrate.exe
This program is a simple wrapper for the Tobii guest calibration tool.
The same as *TobiiCalibrate.exe* it launches the calibration tool, however, the calibration data is stored in a guest profile.

### TobiiTest.exe
This program is a simple wrapper for the Tobii eye tracking testing tool.
It launches a GUI where the result of the calibration can be verified and a new calibration process can be started if required.

### GazeToMouse.exe
This program uses the Tobii Core SDK to get the position on the screen where the user is looking at.
The mouse cursor position is the updated to this position.
As a consequence, the mouse cursor is controlled by the gaze of the user.
This program runs infinitely until it is terminated by an external command.
This should **not** be done with a forced kill (e.g. by executing the command `taskkill /F /IM GazeToMouse.exe` or by killing the task with the task manager) because it prevents the program from gracefully terminating.
This as several consequences:
 - open files are not closed properly and the data stream is cut off. This can lead to corrupt files.
 - if the feature of hiding the mouse pointer is used, the mouse will remain hidden.
 - memory is not freed properly.

Instead `taskkill /IM GazeToMouse.exe` should be used.
This is done in the program `GazeToMouseClose.exe`.

### GazeToMouseClose.exe
This program requests `GazeToMouse.exe` to close gracefully and logs these events to the log file.

### ShowMouse.exe
This program allows to restore the standard mouse pointer.
It might be useful if the program `GazeToMouse.exe` crashes or is closed forcefully such that the mouse pointer is not restored after terminating.
The user might end up with a hidden mouse pointer.
A good solution for such a case is to install a shortcut to `ShowMouse.exe` on the desktop in order to execute it with the keyboard.

## Config File
Each executable of the toolset uses a common config file.
The config file serves to provide the executables with different information such as the location of the Tobii executables.
The config file must be named `config.json` and is read from the following places with the indicated priority:
 1. in the directory of the caller, e.g. the installation folder of `ztree`
 2. in the directory of the executables, i.e. the installation folder of the here described utilities

If no config file can be found, the following default values are used:

    {
        // the path to the output file
        "OutputPath": "",

        // the Tobii installation path
        "TobiiPath": "C:\\Program Files (x86)\\Tobii",

        // Path to the standard mouse pointer icon. This is used to restore the
        // mouse pointer
        "StandardMouseIconPath": "C:\\Windows\\Cursors\\aero_arrow.cur",

        // Tobii EyeX command to run a calibration
        "TobiiCalibrate": "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",

        // Tobii EyeX parameter to run a calibration
        "TobiiCalibrateArguments": "--calibrate",

        // Tobii EyeX command to run a guest calibration
        "TobiiGuestCalibrate": "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",

        // Tobii EyeX parameter to run a guest calibration
        "TobiiGuestCalibrateArguments": "--guest-calibration",

        // Tobii EyeX command to run calibration test
        "TobiiTest": "Tobii EyeX Interaction\\Tobii.EyeX.Interaction.TestEyeTracking.exe",

        // filter settings for the eye tracker
        //   0: unfiltered
        //   1: lightly filtered
        "GazeFilter": 0

        // if set to true the mouse curser will be hidden by GazeToMouse.exe and
        // shown by GazeToMouseClose.exe
        "HideMouse": false
    }

## Output File
When running the program `gazeToMouse.exe` an output file is generated which holds the gaze coordinates of the user.
The output file is saved in the directory specified by `OutputPath` in `config.json` and named `<yyyyMMddTHHmmss>_<hostName>_gaze.txt` where
 - `<yyyyMMddTHHmmss>` is replaced by the timestamp when the file was created (e.g. 20180129085521 stands for 2018.01.12 08:55:21).
 - `<hostName>` is replaced by the name of the machine

## Log File
All executables write continuously to the same log file.
This allows to track the eye tracker events that happened throughout a ztree session within one log file.
The log file is produced at the root directory of the application which is making the calls to the executables (e.g. at the location of `zleaf.exe`).
