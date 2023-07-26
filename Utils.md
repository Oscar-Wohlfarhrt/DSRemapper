# Utils - Class
Namespace: DSRemapper.RemapperLua
Assembly: DSRemapper.RemapperLua.dll

Utils class for the lua remap profiles.
Provides a set of useful functions to remap a controller (for advanced users or not so advanced too).

## Fields

|Type|Name|Description|
|---|---|---|
|const Float|MinAxis|Min controller axis value|
|const Float|CenterAxis|Min controller axis value|
|const Float|MaxAxis|Min controller axis value|
|const Float|MinTrigger|Min controller axis value|
|const Float|MaxTrigger|Min controller axis value|

## Methods

### CreateOutputReport()

Creates a default DSOutputReport object to set physical device feedback properties.


#### Return

|Type|Description|
|---|---|
|DSOutputReport|A default DSOutputReport object|

### CreateSSF()

Creates a simple signal filter to reduce measurement noise from the raw IMU data.


#### Return

|Type|Description|
|---|---|
|SimpleSignalFilter|An inizialized simple signal filter|

### CreateAverange()

Creates an exponetial moving averange.
This allow to get the average from a series of numbers with a minimum memory usage.


#### Return

|Type|Description|
|---|---|
|ExpMovingAverage|An initialized exponetial moving averange|

### CreateAverangeVec3()

Creates an exponetial moving averange for a 3D vector (useful to process IMU data)
This allow to get the average from a series of 3D vectors with a minimum memory usage.


#### Return

|Type|Description|
|---|---|
|ExpMovingAverageVector3|An initialized exponetial moving averange for 3D vector|

### CreateVector2([Single val = 0])

Creates an two dimensional vector

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|val|X and Y value for the vector|

#### Return

|Type|Description|
|---|---|
|DSVector2|An initialized 2D vector|

### CreateVector2(Single x, Single y)

Creates an two dimensional vector

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|x|X value of the vector|
|Float|y|Y value of the vector|

#### Return

|Type|Description|
|---|---|
|DSVector2|An initialized 2D vector|

### CreateVector3([Single val = 0])

Creates an three dimensional vector

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|val|X and Y value for the vector|

#### Return

|Type|Description|
|---|---|
|DSVector3|An initialized 3D vector|

### CreateVector3(Single x, Single y, Single z)

Creates an three dimensional vector

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|x|X value of the vector|
|Float|y|Y value of the vector|
|Float|z|Z value of the vector|

#### Return

|Type|Description|
|---|---|
|DSVector3|An initialized 3D vector|

### CreateQuaternion()

Creates a identity quaternion (X: 0, Y: 0, Z: 0, W: 1)


#### Return

|Type|Description|
|---|---|
|DSQuaternion|A identity quaternion|

### Deadzone(Single value, Single deadzone)

Basic deadzone function to keep axis centered until a threshold

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|
|Float|deadzone|Axis threshold value (it's applied to positive and negative axis values)|

#### Return

|Type|Description|
|---|---|
|Float|Output axis value with deadzone applied|

### NoLinAxis(Single value, Single noLinear)

A nonlinear axis function to convert lineal axis input into an exponential axis output.
Useful to get precision for low axis values, without losing its range.
With this function a mayor movement at the start of the axis is translated into a minor movement of the output axis, and a minor movement at the end of the axis is translated into a mayor movement of the output axis.

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|
|Float|noLinear|Nonlinear factor (0 = linear and 1 = cubic exponential)|

#### Return

|Type|Description|
|---|---|
|Float|Output axis with non-linearization applied|

### LinVecComp(Single value)

Linearizes a sine function, useful for converting normalized vectors components of SixAxis (gravity-vector components) to a linear output.

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|

#### Return

|Type|Description|
|---|---|
|Float|Linearized vector component|

### Abs(Single value)

Absolute function

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|

#### Return

|Type|Description|
|---|---|
|Float|Positive number of the same magnitude of the input number|

### Remap(Single value, Single from1, Single to1, Single from2, Single to2)

Converts one range to another. Converts outside of the input range too.

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|
|Float|from1|Min input range reference|
|Float|to1|Max input range reference|
|Float|from2|Min output range reference|
|Float|to2|Max output range reference|

#### Return

|Type|Description|
|---|---|
|Float|Equivalent of the input value in the output range|

### Clamp(Single value, Single min, Single max)

Constrains a value to the given limits

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|
|Float|min|Min value|
|Float|max|Max value|

#### Return

|Type|Description|
|---|---|
|Float|A value between min and max values|

### ClampAxis(Single value)

Constrains a value to the default axis limits [-1, 1]

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|

#### Return

|Type|Description|
|---|---|
|Float|A value between -1 and 1|

### ClampTrigger(Single value)

Constrains a value to the default trigger limits [0, 1]

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|

#### Return

|Type|Description|
|---|---|
|Float|A value between 0 and 1|

### Sign(Single value)

Gets the sign of a number.

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|

#### Return

|Type|Description|
|---|---|
|Float|-1 for negative numbers and 1 for positive numbers (zero included)|

### SignPow(Single value, Single pow)

A power function which multiplies the sign of the input to the output (to get even powers with negative and positive sign).

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|
|Float|pow|Power value|

#### Return

|Type|Description|
|---|---|
|Float|The input value raised to the power value and multiplied by its sign|

### Bend(Single value, Single bend)

Counter part of the NoLinAxis axis function.

#### Parameters

|Type|Name|Description|
|---|---|---|
|Float|value|Input axis value|
|Float|bend|Cubic factor (0 = cubic exponential and 1 = linear)|

#### Return

|Type|Description|
|---|---|
|Float|Output axis with non-linearization applied|

### SaveTable(String path, Table table)

Saves a lua table to a file

#### Parameters

|Type|Name|Description|
|---|---|---|
|String|path|A path relative to the DSRemapper profile config folder|
|Table|table|Lua table to be saved in the file|

### LoadTable(String path)

Loads a lua table from a file

#### Parameters

|Type|Name|Description|
|---|---|---|
|String|path|A path relative to the DSRemapper profile config folder|

#### Return

|Type|Description|
|---|---|
|Table|Lua table saved in the file|