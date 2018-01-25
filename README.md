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
This should **not** be done with a forced kill (e.g. execute the command `taskkill /F /IM GazeToMouse.exe` or killing the task with the taskmanager) because it prevents the program from gracefully terminating.
This as several consequences:
 - open files are not closed properly and the data stream is cut off. This can lead to corrupt files.
 - if the feature of hiding the mouse pointer is used, the mouse will remain hidden.
 - Memory is not freed properly.

Instead `taskkill /IM GazeToMouse.exe` should be used.
This is done in the program `GazeToMouseClose.exe`.

### GazeToMouseClose.exe
This program requests `GazeToMouse.exe` to close gracefully.

## Config File
Each executable of the toolset uses a common config file.
The config file serves to provide the executables with different information such as the location of the Tobii executables.
The config file must be placed at the root directory of the application which is making the calls to the executables (e.g. at the location of `zleaf.exe`) and must be named `config.json`.
If no config file can be found, the following default values are used:

    {
        // the path to the output file
        "OutputFile": "gaze.data",

        // the Tobii installation path
        "TobiiPath": "C:\\Program Files (x86)\\Tobii\\",

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

        // filter settings for the eye tracker (0: unfiltered, 1: lightly filtered)
        "GazeFilter": 0

        // if set to true the mouse curser will be hidden by GazeToMouse.exe and shown by GazeToMousClose.exe
        "HideMouse": false
    }

## Log File
All executables write continuously to the same log file.
This allows to track the eyetracker events that happened throughout a ztree session within one log file.
The log file is produced at the root directory of the application which is making the calls to the executables (e.g. at the location of `zleaf.exe`).
