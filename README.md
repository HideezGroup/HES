Hideez Enterprise Server
========================

The Hideez Enterprise Server (HES) is a web application intended to manage the Hideez Keys Enterprise Edition (Hideez Key EE).Hideez Key EE is a hardware security token intended to store users' credentials. It has Bluetooth Low Energy (BLE) interface to connect to host computers. It can be used to unlock and lock Windows PC by BLE proximity (distance between the user and his PC) and as a hardware password manager and One Time Password (OTP) generator. Hideez Client application is used to connect and manage Hideez Keys EE on a local PC. Hideez Client can lock/unlock your PC and enter your logins, passwords, and OTP into web sites and desktop applications, getting them from the Hideez Key EE. With the HES, you can securely transfer credentials from the web directly to a Hideez Key; manage employee list; assign Hideez Keys to an employee; securely transfer credentials to the Hideez Key; view statistics and audit reports; manage shared accounts and more. To start working with HES, you need:
- At least one Hideez Key EE and one Hideez Dongle (USB radio module to connect the Key). You can order it from the Hideez web site [Order a Pilot](https://hideez.com/pages/hideez-enterprise#order-hes).
- Linux or Windows machine to install the HES.
- Windows 7/10 PC to install Hideez Client application – you can build it from sources or request from the Hideez.
- Windows Active Directory domain (Optionally).

[Learn about Hideez Enterprise Solution](https://hideez.com/pages/hideez-enterprise).

User documention about [Hideez Enterprise Solution](https://support.hideez.com/hideez-enterprise-server). 

## Supported platforms

At this time, HES can be installed on Linux or Windows computer. Hideez Client can be installed on Windows 7/10 PC. Currently, we don't support mobile or Mac platforms but stay tuned for the updates.

## System requirements

HES: Linux or Windows with .NET Core (.NET Core SDK version 5.0) installed, MySQL database server. More requirements see on the corresponding platform setup instructions. Hideez Client: Windows 7/10, one free USB port for a Hideez Dongle.

## Get Started

To deploy the server on Linux, follow instructions in the [Linux deployment](LINUX.md).

To deploy the server on Linux from a Docker containar, follow instructions in the [Linux Docker deployment](HES.Docker/README.md).

To deploy the server on Windows, follow instructions in the [Windows deployment](WINDOWS.md).

After you have deployed the server open the HES web interface and log in using the default login 'admin@hideez.com' and default password 'admin'.

## API documentation

Almost everything you can do with the HES web interface you can do via the HES REST API calls. Follow the `Quick Start` instructions in the [documentation](API.md).

## Next steps

- invite more admins and delete default login 'admin@hideez.com'
- setup the HES following the [user documentation](https://support.hideez.com/hideez-enterprise-server)

## Related projects

These are repos for related projects:
* [Hideez Client](https://github.com/HideezGroup/win.HideezSafe) - Connection with the device, entering credentials and OTP.
