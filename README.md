# Toolset to Control Tobii Eye Tracker 

This repository contains the source code for multiple simple tools that allow to control a Tobii eye tracker from a 3rd party application.
Specifically, this project aims at providing a set of executables that can be called from within [ztree](http://www.ztree.uzh.ch/en.html) to allow eye tracker support for economic experiments.

For more details please refer to the [documentation](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/doc/tutorial.pdf).

## Installation
The complete toolset package can be downloaded from the [release folder](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/release).
The package contains the following executables:

 - **CustomCalibrate.exe** launches the custom calibration tool.
 - **TobiiCalibrate.exe** launches the Tobii calibration tool.
 - **Gaze.exe** allows to record gaze data and control the mouse with the gaze of the user. The following executables provide some runtime control of a running `Gaze.exe` process:
	 - **GazeClose.exe** terminates `Gaze.exe` gracefully.
	 - **GazeRecordingDisable.exe** pauses the gaze recording.
	 - **GazeRecordingEnable.exe** resumes the gaze recording.
	 - **MouseTrackingDisable.exe** disables the link between the gaze and the mouse position.
	 - **MouseTrackingEnable.exe** enables the link between the gaze and the mouse position.
 - **ShowMouse.exe** restores a hidden mouse pointer if something went wrong

In order to run the executables the following files need to be placed in the same directory as the executables:

 - `tobii_pro.dll`
 - `tobii_firmware_upgrade.dll`
 - `assets/blank.cur`
 - `config.json`


Further, the Tobii engine must be running and the eye tracker must be enabled.

### Tobii Eye Tracker 4c
To install the driver for the [Tobii Eye Tracker 4c](https://tobiigaming.com/eye-tracker-4c/) install [Tobii Experience Driver](https://files.update.tech.tobii.com/Tobii.IS4C.Offline.Installer_4.124.0.15937.msi).

This will start the following services:
- `Tobii Runtime Service`
- `Tobii Service`

and the following processs:
- `Tobii Interaction Engine`

### Tobii Pro Spark
To install the driver for the [Tobii Pro Spark](https://www.tobii.com/products/eye-trackers/screen-based/tobii-pro-spark) use the [Tobii Pro Eye Tracker Manager](https://www.tobii.com/products/software/applications-and-developer-kits/tobii-pro-eye-tracker-manager):

1. Install Tobii Pro Eye Tracker Manager (ETM)
2. Connect the Tobii Pro SPark device to the computer
3. Install the driver with the ETM

This starts the service `Tobii Pro Spark Runtime`.

## 3rd Party Applications

This section provides some infromation on how to run the here provided executables from within 3rd party applications.

### ztree

For quick starters, a simple [``ztree`` sample program](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/sample/template.ztt) is available.

### Opensesame

To start a process from within opensesame use a python script.
The following example starts the custom calibartion program with the subject number passed as argument:

```python
import subprocess

subprocess.run(["CustomCalibrate.exe", "/subject", var.get(u'subject_nr')])
```

## Release Notes
Information about the releases can be found in the [CHANGELOG](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/CHANGELOG.md)
