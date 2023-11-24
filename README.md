# Toolset to Control Tobii Eye Tracker 

This repository contains the source code for multiple simple tools that allow to control a Tobii eye tracker from a 3rd party application.
Specifically, this project aims at providing a set of executables that can be called from within [ztree](http://www.ztree.uzh.ch/en.html) to allow eye tracker support for economic experiments.

For more details please refer to the [documentation](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/doc/tutorial.pdf).

**Important** Configure the task manager to be always in the foreground (In task manager enable "Options->Always on top").
Why: The application `Gaze.exe` may open windows that are put to the foreground in a very aggressive manner.
This is done in order to cope with experimentation software that uses this same behaviour (e.g. Opensesame with psychopy or expyriment backend).
If something goes wrong with `Gaze.exe` the user could be locked out from the computer because a window keeps blocking access to the system.
With the task manager set to "Always on top" there is a way out.

## Installation
The complete toolset package can be downloaded from the [releases](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/tags).
The package contains the following executables:

- **`Gaze.exe`** This program uses the [Tobii Pro SDK](http://developer.tobii.com/tobii-pro-sdk/) to extract the gaze position on the screen where the subject is looking at.
    The extracted data is recorded and stored to a file.
    Optionally, the mouse cursor position is updated to this position such that the mouse cursor is controlled by the gaze of the subject.
    Instead of using an eye tracker device it is also possible to simply log the mouse coordinates.
  **`Gaze.exe`** runs infinitely until it is terminated by an external command.
    This should **not** be done with a forced kill (e.g. by executing the command `taskkill /F /IM Gaze.exe` or by killing the task with the task manager) because it prevents the program from terminating gracefully.
    This as several consequences:
    - open files are not closed properly and the data stream is cut off. This can lead to corrupt files.
    - if the feature of hiding the mouse pointer is used, the mouse will remain hidden.
    - memory is not freed properly.
  Instead the program **`GazeControls.exe /command TERMINATE`** should be used.
- **`GazeControl.exe`** This program allows to interact with **`Gaze.exe`**. `GazeControl.exe` accepts the following optional arguments:
    - `/reset`: Allows to reset the relative timestamp of the gaze data.
    - `/trialId <ID>`: Sets a trial ID `<ID>` which will be added to each data sample in the output file. **Important**: Make sure that only integer numbers are used as trial ID.
    - `/label <LABEL>`: Sets a custom label `<LABEL>` which will be added to each data sample in the output file. Any string is accepted here.
    - `/command <COMMAND>`: A command allow to activate/deactivate features of `Gaze.exe`. The following commands are supported:
        - `CUSTOM_CALIBRATE` uses the [Tobii Pro SDK](http://developer.tobii.com/tobii-pro-sdk/) and launches a custom calibration process which allows to calibrate the eye tracker without having to rely on the calibration software provided by Tobii.
        - `VALIDATE` uses the [Tobii Pro SDK Addon](https://github.com/tobiipro/prosdk-addons-dotnet) and launches a validation process.
        - `DRIFT_COMPENSATION` launches a custom drift compensation process to compensate gaze drifts that may occur during experimentation.
        - `GAZE_RECORDING_DISABLE` requests **`Gaze.exe`** to stop recording gaze data.
            `Gaze.exe` will continue to run (and update the mouse pointer if configured accordingly) but no longer store gaze data to the disk.
        - `GAZE_RECORDING_ENABLE` requests **`Gaze.exe`** to start recording gaze data.
        - `MOUSE_TRACKING_DISABLE` requests **`Gaze.exe`** to stop updating the mouse pointer by the gaze position.
        - `MOUSE_TRACKING_ENABLE` requests **`Gaze.exe`** to start updating the mouse pointer by the gaze position.
        - `RESET_DRIFT_COMPENSATION` resets the drift compensation computed with the command `DRIFT_COMPENSATION`.
        - `TERMINATE` requests **`Gaze.exe`** to close gracefully and logs these events to the log file.
        
    Multiple arguments can be passed to the application but each argument can only be passed once.
    Passing an argument to an application can be done in command line or by crating a shortcut to the program.
    Corresponding shortcuts for all available `<COMMAND>`s are provided in the release package.
- **`ShowMouse.exe`** This program allows to restore the standard mouse pointer.
    It might be useful if the program `Gaze.exe` crashes or is closed forcefully such that the mouse pointer is not restored after terminating.
    The subject might end up with a hidden mouse pointer.
    A good solution for such a case is to install a shortcut to `ShowMouse.exe` on the desktop in order to execute it with the keyboard.

In order to run the executables the following files need to be placed in the same directory as the executables:

 - `tobii_pro.dll`
 - `tobii_firmware_upgrade.dll`
 - `assets/blank.cur`
 - `config.json`

In order to use tje GazeControlLibrary, the following files need to be placed in the same directory as the executables:
 - `GazeControlLibrary.dll`
 - `Newtonsoft.Json.dll`

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

### Scripts

The folder `scripts` contains two files `CreateShortcut.ps1` and `CreateShortcuts.bat` which allow to create shortcuts to the application `GazeControl.exe` with predefined command arguments.
In order to generate the shortcut files perform the following steps:

1. copy the two script files into the installation folder
1. execute the script `CreateShortcuts.bat`

Nothe that the generated shortcuts are tied to the installation folder.
Copying the installation folder to another location will break the links.

## 3rd Party Applications

This section provides some infromation on how to run the here provided executables from within 3rd party applications.

### ztree

For quick starters, a simple [``ztree`` sample program](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/sample/template.ztt) is available.

### Opensesame

For quick starters, a simple [``opensesame`` sample program](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/sample/template4.0.osexp) is available.

## Release Notes
Information about the releases can be found in the [CHANGELOG](http://phhum-a209-cp.unibe.ch:10012/TBI/TBI-tobii_eye_tracker_gaze/blob/master/CHANGELOG.md)
