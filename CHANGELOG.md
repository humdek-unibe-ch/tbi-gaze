# Changelog

## v3.5.3
### New Features
- Allow to configure the `ForegroundColor` of the windows. The foreground
  color changes the text color and the calibration point color.

## v3.5.2
### New Features
- Allow to enable/disabel the systray icon with the configuration option
  `EnableSystrayIcon`. By default the icon is enabled.

### Bug Fixes
- Fix a problem where cSolor values were never read from the config file.

## v3.5.1
### New Features
- Add the commadn `LOAD` to show a window with a spinner. Configure the
  duration of the how long the window is shown with the configuration option
  `LoadingTimer`.

### Bug Fixes
- Fix typo in config sample.

## v3.5.0
### New Features
- Allow to configure the `BackgroundColor` and the `FrameColor` of the windows
  (#8).
- Calibration accuracy
    - Estimate the calibration accuracy by capturing gaze data during the
      calibration process and associating the captured data to the calibration
      result.
    - Add the calibration accuracy values to the calibration data output (#6).
    - Show an alert in the calibration result view if the calibration accuracy
      is poor. The threshold can be configured with
      `CalibrationAccuracyThreshold` (#6).
    - Allow to configure automatic calibration restart with the configuration
      item `CalibrationRetries` (#6).
- Validation accuracy and precision
    - Show an alert in the validation result view if the validation accuracy or
      precision is poor. The thresholds can be configured with
      `ValidationAccuracyThreshold` and `ValidationPrecisionThreshold`,
      respecvely (#6).
    - Allow to configure automatic validation restart with the configuration
      item `ValidationRetries` (#6).
- Improve calibration result view:
    - Don't display gaze points for failed calibration points.
    - Color failed calibration points red in calibration result view.
    - Draw fine lines from gaze points to the corresponding calibration points
      to allow for an easy association.
- `GazeControl.exe` returns now an error code which allows to check whether the
  command was executed correctly.
- Add a systray icon. This allows to control the application without having to
  use the `GazeControl` application.

### Improvements
- Place log files in folder `log` (#2).
- Add log entries for pipe server (#3).
- Skip failed calibration point and continue calibration.
- Check and sanitize calibration and validation points to avoid setting the
  same point multiple times.

### Changes
- Changes to the calibration output. This requires a new value for the
  configuration items `CalibrationLogColumnTitle` and (optionally)
  `CalibrationLogColumnOrder`.

### Bug Fixes
- Fix keyboard focus when a window is opened (#11).
- Fix validation data output (y values of the validation points).

## v3.4.2
### Improvements
- Seperate pipe command handler funtionality to `NamedPipeClient` class and
  move `NamedPipeClient` to seperate project based on .NET 4.72. This allows
  for a direct integration in python with `pythonnet`.

### Bug Fixes
- Log GazeControl messages to seperate file to avoid race conditions (#1).
- Fix pipe connection problems on startup (#5).
- Only hide the mouse when calibration dots are shown (#7).
- Fix dispersion calculation in drift compensation (#9).

## v3.4.1
### Improvements
- Add note on foreground behaviour to readme.

### Bug Fixes
- Fix validation when only one screen is connected.

## v3.4.0
### New Features
- Add configuration option `DriftCompensationWindowShow` to enable or disable
  the drift compensation window.
- Add configuration option `DriftCompensationDurationThreshold` to configure
  the required fixation time during drift compensation.
- Add configuration option `DriftCompensationWindowShow` to enable or disable
  the drift compensation window.
- Add configuration option `DriftCompensationDispersionThresholdMax` which
  allows to define maximal allowed deviation angle during drift compensation:
  If the computed compensation angle is larger as the here configured angle the
  drift compensation is not updated.
- During `Gaze.exe` startup check for already running `Gaze` processes and kill
  them.

### Improvements
- Update default configuration settings to allow starting the application
  without error.
- Improvments to `GazeControl.exe` and annotations:
    - Introduce arguments `/label` and `/trialId` to pass a label and the trial
      ID.
    - Make argument `/command` optional to allow setting annotations without
      a command.
    - Set trialID and label based on capture timestamp instead of system
      timestamp.
- Improve timestamps: Compute the tracker latency for each sample and use this
  to dump the system time of data capure and the system time of data reading.
- Update opensesame templates. This might help to reduce taskbar flickering and
  makes the code more maintainable.

### Changes
- Remove argument `/value` in `GazeControl.exe` because it became obsolete with
  the new arguments `/label` and `trialId`.
- Rename configuration option `DispersionThreshold` to
  `DriftCompensationDispersionThreshold`.
- Hide window icon in taskbar. This might help to avoid taskbar flickering
  during experimentatino.
- Save data files with `csv` extension instead of `txt`.
- By default, use `,` delimiter instead of `\t` delimiter when dumping values.

### Bug Fixes
- Fix dispersion computation during drift compensation.

## v3.3.2
### New Features
- Allow to select the screen on a multi-screen setup with the keyboard. 

### Improvements
- Improve opensesame template files and add a template for version 3.3 and 4.0.
- Improve ztree template file
- Add a documentation to the sample folder

### Bug Fixes
- Move calibration, validation and drift compensation windows to the foreground.

## v3.3.1
### Improvements
- Improve performance.
- Dump validation results for each validation point.

### Bug Fixes
- Represent the relative timestamp in total milliseconds instead of a timespan.

## v3.3.0
### New Features
- Add pipe command `SET_TRIAL_ID` to allow to annotate data samples.
- On multi-screen setups start calibration with a screen selection page.
- Add Screen Area coordinates to the dumped configuration file.
- Add gaze validation which can be started through the command `VALIDATE`.

### Improvements
- Fix compiler warnings.
- Extend helper scripts. 

## v3.2.0
### New Features
- Add relative timestamp to output data.
- Add annotation tag to ouput data.
- Add pipe command `SET_TAG` to allow annotate data samples.
- Add pipe command `RESET_START_TIMER` to reste the relative timestamp.
- Add a log entry of the version of the gaze application.
- Add helper scripts to generate shortcuts to `GazeControl.exe`.

## v3.1.0
### New Features
 - Add a custom drift compensation process
 - Allow to pass the argument `outputPath` to the application for dynamic
   output path assignement.

### Improvements
 - Integrate calibration into `Gaze.exe`
 - Remove Tobii research dependencies from everywhere except the eye tracker
   device class
 - Cleanup and rearrangement of code to improve readability

## v3.0.0
### New Features
 - A custom calibration application is added to the portfolio. This allows to
   calibrate a device without the need for a 3rd party application.
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
 - Remove all configuration options for Tobii Core (only Tobii Pro SDK is
   supported)
 - Remove Tobii Core application wrapper (TobiiTest, TobiiGuestCalibrate)
 - Use the Tobii pro eye tracker manager for device calibration instead of the
   Tobii Core software.
 - Rename `GazeToMouse` to `Gaze` and `GazeToMouseClose` to `GazeClose`.

## v2.3.0
### New Features
 - A mouse tracker device can now be used instead of an eyetracker device. The
   mouse tracker logs the timestamp and the x and y coordinates of the mouse
   pointer whenever the mouse-move event is fired. The mouse tracker is used
   when the configuration filed 'TrackerDevice' is set to the value 2.

### Improvements
 - Rename the configuration field 'TobiiSDK' to 'TrackerDevice'.

## v2.2.0
### New Features
 - Configuration file
   - Dump the configurations used for an experiment to a file at the
     "DataLogPath"
   - Allow to configure an experiment name which is used as a postfix of the
     dumped configuration file name
   - Consider the config file as invalid if not all required configuration
     parameters are defined
   - Consider the config file as invalid if unknown parameters are defined
   - Allow to configure whether to log data sets where all data is invalid
     (eyes closed, no subject in front of the screen, etc)
 - Error Handling
   - Attach an error string to the output file, indicating errors that occurred
     during the run
   - Attach an error string to the dumped configuration file, indicating errors
     of the configuration

### Improvements
 - Fall back to Core SDK if the license file cannot be applied to the device

## v2.1.0
### New Features
 - Log eye origin coordinates
   - x, y, z coordinates of the left and the right eye
   - compute distance of the left and right eye to the eyetracker
   - compute the average distance of the two eyes

### Improvements
 - Check the three format values and the column order individually to produce
   more specific log entries

## v2.0.1
### Bug Fix
 - with SDK Pro, use system timestamp to cope with disconnected device
 - fix the path in the z-tree sample file

## v2.0.0
### New Features
 - Support for Tobii Pro SDK
   - apply license to eyetracker device at stratup
   - logging of pupil diameter
   - logging of individual eye data
 - Allow to configure column headers of output file

### Improvements
 - Improved configuration options for the output file

## v1.0.0
### New Features
 - Notify user with popup if eyetracker is not ready
 - Allow to configure time interval for the software to wait for the eyetracker
   to become ready

### Improvements
 - Rename default output file for data from ```<prefix>_data.txt``` to
   ``<prefix>_gaze.txt``

## v0.3.2
### Improvements

 - add header to the data log file.
 - change the default value of allowed gaze data files.
 - check and wait for ready state of the eye tracker before performing
   operations with it.

### Bug Fix

 - create a log file per machine to prevent concurrency conflicts.

## v0.3.1

### Improvements

 - ignore the option "HideMouse" when "ControlMouse" is disabled.

### Bug Fix

 - remove double log entry of mouse hiding and restoring event.

## v0.3.0

### New Features

 - allow to configure whether the gaze data is logged.
 - allow to configure the maximum allowed amount of gaze data files in the
   output folder.
   Oldest files are deleted first.

### Improvements

 - limit the logfile size to 1MB.
   If the size is exceeded a new file is created.
   At any time only two log files are allowed,
   The older file is overwritten once both files exceed 1MB.

## v0.2.0

### New Features

 - allow to configure whether the mouse is controlled by the gaze of the
   subject or not.
 - allow to configure the output format of the gaze data.


## v0.1.0

First release of the GazeToMouse toolset.

The toolset was tested on **Windows 7** in conjunction with **ztree v3.6.7**
and Tobii **Eye Tracking Core v2.11.1.6952**.


