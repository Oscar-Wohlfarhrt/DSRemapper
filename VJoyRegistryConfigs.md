# VJoy Registry Configurations

One day I want to play Snow Runner and I discover it only detects devices which are identified as steering wheels, then i start searching for a solution to this problem, and I found one.

The awnswer was to edit a windows registry key which, I asume, is the device identifier or something like that, basicaly changing the following register key the Snow Runner start detecting the VJoy Virtual Controller as a stearing wheel.

Register Key: `[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\VID_1234&PID_BEAD]`

Key Value: `OEMData`
Value for normal working: `03 00 88 11 20 00 00 00`
Value for stearing wheel: `43 00 88 01 fe 00 00 00`
