# FirmwareGen

FirmwareGen (or Firmware Generator) is a tool designed to fully automate and assist in building fully fledged ready to go Full Flash Update (FFU) images for devices supported as part of the WOA-Project organisation.

Namely, the FirmwareGen tool is designed to setup prep and create FFU files containing Windows Desktop for ARM64 Processors for specific phone based devices:

- Lumia 950
- Lumia 950 XL
- RX-130 EB0.5, EB1.0, EB2.0, EB2.1, EB2.2 and EB2.3
- Surface Duo (1st Gen) 128GB, 256GB
- Surface Duo 2 128GB, 256GB, 512GB
- Various Qualcomm Reference Platforms

This tool is not specifically hardcoded to target those exact devices and can be easily extended without forking it or even modifying it by simply authoring device profile xml files.

In a nut shell the tool is going to:

- Generate a Virtual Hard Disk matching the specifications of the device profile
- Generate the target's wanted GUID Partition Table on said Virtual Disk
- Apply a Windows Desktop Operating System Image from a provided installation media
- Configure the ESP
- Install Drivers
- Package it all up as a FFU file ready to flash on the given device