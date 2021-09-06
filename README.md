# TUFUpdater

Helper utility for flashing ATmega32u4 boards.

TUFUpdater queries WMI for available serial ports for 30 seconds and detects if any boards that match the VID/PID combos in `data/boards.json` are available. Once a recognized board is available, TUFUpdater will run a Windows fork of avrdude to flash your board. This should remove the need to install any special drivers and should "just work © ℗®™".

## Usage

1. Extract the `TUFUpdater.zip` file to a folder on your machine.
1. Copy your compiled `.hex` file into the `TUFUpdater` folder.
1. Navigate into the folder, then drag and drop your `.hex` file onto `TUFUpdater.exe`. You will see this prompt:

![Screenshot 1](/.assets/images/screenshot1.png)

1. Follow the instructions and you should see the output from avrdude about the status of the flash update.

![Screenshot 2](/.assets/images/screenshot2.png)

## Build/Publish

Publish:

```sh
dotnet publish -c Release -r win-x64 /p:TargetFramework=net5.0
```
