# DSRemapper

[![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)  ![Build Status](https://img.shields.io/badge/Build-Pre--Alpha-orange)

DSRemapper is an open source application to remap PC controllers with written scripts and with plugins support to extend its functionalities.

`Aided with Google Translate (my native language is Spanish, I'm from Argentina)`

---

## Inspiration
I love playing games, especially simulators. Driven by this I found InputMapper, an application that allowed me to get input from a PS4 controller (DualShock4 - DS4) and remap it into an Xbox360 controller. One feature that this application has is to read the DualShock4's accelerometer and gyroscope, and remap it as a joystick input. This was amazing, literally by tilting the controller, and with great reading accuracy, it was able to emulate a steering wheel, which I used in games like ETS2 (Euro Truck Simulator 2).

Unfortunately, the latest versions of the program didn't work as I liked and basically it wasn't very easy to achieve the behavior I was looking for.

As a result, I started creating DSRemapper, an alternative/continuation, but with more freedom, at the cost that DSRemapper may not be for everyone.

## What is DSRemapper?

DSRemapper is an open source application to remap PC controllers with written scripts.

These scripts can be written in lua, which is the way I designed the program. I focused on making the syntax easy for everyone. Although I understand that programming is not for everyone, it shouldn't be difficult to modify or write a basic script to achieve what you want without too much trouble.

However, I plan to design an interface system that makes this procedure simpler for everyone, by generating a lua script automatically. This would allow DSRemapper to reach more people and still give complete freedom to everyone who wants it.

## Plugins?

Yes, DSRemapper is based on plugins, so if you want DSRemapper to do something new, and you have the ability to do it, this program provides a way to create:

- **New Remapper Plugins**, which are for read a file with the instructions of how to remap the physical device to a virtual one
- **New Input Plugins**, which recognize physical devices and translate it's input for remap it.
- **New Output Plugins**, which create virtual devices or emulate physical devices to give a certain behavior to the physical devices

---

## How to Install it
To run DSRemapper you need to install [.NET 7.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

After that, you have to download the lasted [Release]() of the program, unzip and run.

If you want to use specific output plugins, you need to download the drivers to use it, if they are required.

### Drivers for included Output Plugins

#### ViGEm

Emulates Xbox 360 and DualShock 4 Controllers.

[Official Website](https://vigem.org)
[Download Page](https://vigem.org/Downloads/)
[GitHub](https://github.com/ViGEm/ViGEm.github.io)

#### VJoy

Emulates standard DirectInput controllers.

##### Original
[Official Website](https://sourceforge.net/projects/vjoystick/)

##### Used for the plugin
[GitHub](https://github.com/jshafer817/vJoy)

---

<details><summary><h2>Program Versions Description</h2></summary>
<p>

It may be very obvious how the different versions work, but I wanted to give more details about what can happen in each version.

### Pre-Alpha `[Current]`

The program can suffer major changes from one version to another and be unstable.
Some changes can be:

#### For users

- The lua remapper syntax for controller remap can change
- Interface can be reworked (Again, there are three going)

#### For developers

- Renaming of classes, structs, enums, etc. of the Core Assembly for plugins

### Alpha

Still unstable, but there will be no more changes that modify the behavior of the program. Could happen that the lua remapper syntax changes slightly.

### Beta

Stable but still not finished.

### Release

Full released app.

</p>
</details>
