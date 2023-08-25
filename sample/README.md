# Sample Files for Experimentation with Eye Tracker Utility

This folder hold some sample files to use the gaze utility in a experiment management tool.

## `config.json`

A sample configuration file which can be used as a starting point to configure the gaze utility.

## `template.osexp`

A sample file which demonstrates how to start the gaze utility from openseame.
This was tested on opensesame version `3.3.14` and `4.0.5` on Windows.

Note that the application only worked with the PyGame (legacy) backend because otherwise the gaze windows kept beeing covered by the opensesame fullscreen window.

It might be possible (and potentially a better solution) to manually control the window through python (e.g. with win32gui on Windows or with xdotool on Linux).

## `template.ztt`

A sample file which demonstrates how to start the gaze utility from openseame.
The sample file was generated with the ytree version 5.1.11. 
