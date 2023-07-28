# v3.2.0
### New Features
- Add relative timestamp to output data.
- Add annotation tag to ouput data.
- Add pipe command `SET_TAG` to allow annotate data samples.
- Add pipe command `RESET_START_TIMER` to reste the relative timestamp.
- Add a log entry of the version of the gaze application.
- Add helper scripts to generate shortcuts to `GazeControl.exe`.

# v3.1.0
### New Features
 - Add a custom drift compensation process
 - Allow to pass the argument `outputPath` to the application for dynamic output path assignement.

### Improvements
 - Integrate calibration into `Gaze.exe`
 - Remove Tobii research dependencies from everywhere except the eye tracker device class
 - Cleanup and rearrangement of code to improve readability

# v3.0.0
### New Features
 - A custom calibration application is added to the portfolio.
   This allows to calibrate a device without the need for a 3rd party application.
 - Proper shutdown handling of `GazeToMouse` through named pipes.
 - Allow to enable/disable gaze recording through named pipes.
 - Allow to enable/disable mouse tracking through named pipes.
 - Allow to pass argument subject to the application.

### Improvements
 - Update all projects to .NET version 6.0.
 - Cleanup code base, split functions into seperate libraries.
 - Apply MVVM architectural pattern where sensible.

### Changes
 - Remove Tobii Interaction Library
 - Remove all configuration options for Tobii Core (only Tobii Pro SDK is supported)
 - Remove Tobii Core application wrapper (TobiiTest, TobiiGuestCalibrate)
 - Use the Tobii pro eye tracker manager for device calibration instead of the Tobii Core software.
 - Rename `GazeToMouse` to `Gaze` and `GazeToMouseClose` to `GazeClose`.

# v2.3.0
### New Features
 - A mouse tracker device can now be used instead of an eyetracker device.
   The mouse tracker logs the timestamp and the x and y coordinates of the mouse pointer whenever the mouse-move event is fired.
   The mouse tracker is used when the configuration filed 'TrackerDevice' is set to the value 2.

### Improvements
 - Rename the configuration field 'TobiiSDK' to 'TrackerDevice'.

# v2.2.0
### New Features
 - Configuration file
   - Dump the configurations used for an experiment to a file at the "DataLogPath"
   - Allow to configure an experiment name which is used as a postfix of the dumped configuration file name
   - Consider the config file as invalid if not all required configuration parameters are defined
   - Consider the config file as invalid if unknown parameters are defined
   - Allow to configure whether to log data sets where all data is invalid (eyes closed, no subject in front of the screen, etc)
 - Error Handling
   - Attach an error string to the output file, indicating errors that occurred during the run
   - Attach an error string to the dumped configuration file, indicating errors of the configuration

### Improvements
 - Fall back to Core SDK if the license file cannot be applied to the device

# v2.1.0
### New Features
 - Log eye origin coordinates
   - x, y, z coordinates of the left and the right eye
   - compute distance of the left and right eye to the eyetracker
   - compute the average distance of the two eyes

### Improvements
 - Check the three format values and the column order individually to produce more specific log entries

# v2.0.1
### Bug Fix
 - with SDK Pro, use system timestamp to cope with disconnected device
 - fix the path in the z-tree sample file

# v2.0.0
### New Features
 - Support for Tobii Pro SDK
   - apply license to eyetracker device at stratup
   - logging of pupil diameter
   - logging of individual eye data
 - Allow to configure column headers of output file

### Improvements
 - Improved configuration options for the output file

# v1.0.0
### New Features
 - Notify user with popup if eyetracker is not ready
 - Allow to configure time interval for the software to wait for the eyetracker to become ready

### Improvements
 - Rename default output file for data from ```<prefix>_data.txt``` to ``<prefix>_gaze.txt``

# v0.3.2
### Improvements

 - add header to the data log file.
 - change the default value of allowed gaze data files.
 - check and wait for ready state of the eye tracker before performing operations with it.

### Bug Fix

 - create a log file per machine to prevent concurrency conflicts.

# v0.3.1

### Improvements

 - ignore the option "HideMouse" when "ControlMouse" is disabled.

### Bug Fix

 - remove double log entry of mouse hiding and restoring event.

# v0.3.0

### New Features

 - allow to configure whether the gaze data is logged.
 - allow to configure the maximum allowed amount of gaze data files in the output folder.
   Oldest files are deleted first.

### Improvements

 - limit the logfile size to 1MB.
   If the size is exceeded a new file is created.
   At any time only two log files are allowed,
   The older file is overwritten once both files exceed 1MB.

# v0.2.0

### New Features

 - allow to configure whether the mouse is controlled by the gaze of the subject or not.
 - allow to configure the output format of the gaze data.


# v0.1.0

First release of the GazeToMouse toolset.

The toolset was tested on **Windows 7** in conjunction with **ztree v3.6.7** and Tobii **Eye Tracking Core v2.11.1.6952**.


