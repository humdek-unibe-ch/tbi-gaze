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

### Changes
 - Improved configuration options for the output file

# v1.0.0
### New Features
 - Notify user with popup if eyetracker is not ready
 - Allow to configure time interval for the software to wait for the eyetracker to become ready

### Changes
 - Rename default output file for data from **<prefix>_data.txt** to **<prefix>_gaze.txt**

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


