# Toolset to control Tobii Eye Tracker 4c

This repository contains the source code for multiple simple tools that allow to control a Tobii eye tracker from a 3rd party application.
Specifically, this project aims at providing a set of executables that can be called from within [ztree](http://www.ztree.uzh.ch/en.html) to allow eye tracker support for economic experiments.

The documentation can be found [here](http://tpf.fluido.as:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/doc/tutorial.pdf).

## Installation
The complete toolset package can be downloaded from [here](http://tpf.fluido.as:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/release/v0.1.0.zip).
The package contains the following executables:

 - **TobiiCalibrate.exe** launches the Tobii calibration tool
 - **TobiiGuestCalibrate.exe** launches the Tobii guest calibration
 - **TobiiTest.exe** launches the Tobii test application
 - **GazeToMouse.exe** enables the control of the mouse with the gaze of the user
 - **GazeToMouseClose.exe** terminates ``GazeToMouse.exe`` gracefully
 - **ShowMouse.exe** restores to mouse pointer if something went wrong

In order to run the executables the following files need to be placed in the same directory as the executables:

 - GazeHelper.dll
 - Newtonsoft.Json.dll
 - Tobii.EyeX.Client.dll
 - Tobii.Interaction.Model.dll
 - Tobii.Interaction.Net.dll
 - blank.cur
 - config.json


Further, the Tobii engine must be running and the eye tracker must be enabled.
To install the Tobii engine for the [Tobii Eye Tracker 4c](https://tobiigaming.com/eye-tracker-4c/) download the software from [here](https://tobiigaming.com/downloadlatest/?bundle=tobii-core).

The Tobii engine starts the following service

    Tobii.Service

and the following processes

    Tobii EyeX Engine
    Tobii.EyeX.Interaction.exe
    Tobii.EyeX.Tray.exe

For quick starters, a simple [``ztree`` sample program](http://tpf.fluido.as:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/sample/template.ztt) is available.
