## v0.1.0-pre-alpha
- Started source code documentation.
### Remap profile changes
- Emulated controller state property changed from `state` to `State`.
#### Orthographic errors
- Axes array is now correctly renamed from `Axis` to `Axes` (still will be a readonly definition for `Axis` some more versions for back compatibility with old remap profiles).
- SetAxes function is now correctly renamed from `SetAxis` to `SetAxes` (still will be a readonly definition for `SetAxis` some more versions for back compatibility with old remap profiles).
### Program changes
- The DSInput module was removed, since it contained only one, and lonely, function
### Internal plugins changes
- COMM input plugin now will use Windows native libraries for comunication.
### DSR-SDK package changes
- Started documentation for Core package
