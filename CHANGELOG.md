# v0.3.0

## New features

 - allow to configure whether the gaze data is logged
 - allow to configure the maximum allowed amount of gaze data files in the output folder.
   Oldest files are deleted first.

## Improvements

 - limit the logfile size to 1MB.
   If the size is exceeded a new file is created.
   At any time only two log files are allowed,
   The older file is overwritten once both files exceed 1MB.

# v0.2.0

## New features

 - allow to configure whether the mouse is controlled by the gaze of the subject or not
 - allow to configure the output format of the gaze data


# v0.1.0

First release of the GazeToMouse toolset.

The toolset was tested on **Windows 7** in conjunction with **ztree v3.6.7** and Tobii **Eye Tracking Core v2.11.1.6952**.

