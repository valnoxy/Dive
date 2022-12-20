<h1 align="center"><br><img src="https://dl.exploitox.de/other/dive.png" alt="Dive Logo" width=100></h1>

<h3 align="center">Dive - Deployment is very easy</h3>
<p align="center">
    Easy Deployment for Windows
    <br />
    <strong>Version: </strong>1.0.0.289 (Beta 4)
    <br/>
    <strong>Core: </strong>12.0.0.426
    <br />
    <br />
    <a href="https://github.com/valnoxy/dive/releases"><strong>Download now »</strong></a>
    <br />
    <br />
    <a href="https://github.com/valnoxy/dive/issues">Report Bug</a>
    ·
    <a href="https://github.com/valnoxy/dive/discussions/">Discussions</a>
  </p>
</p>

![-----------------------------------------------------](https://dl.exploitox.de/t440p-oc/rainbow.png)

## 🔔 Information
Dive (formally deploya) is a little application for deploying Windows on a machine. 

- Deploy WIM-Files on any drive
- Mass deploy Windows with AutoDive (W.I.P)
- Bypass Windows 11 Microsoft-Account compulsion
- Capture Windows installation (W.I.P)
- Install Windows from the Cloud

## 🔧 Usage
1. Download the latest version from [GitHub](https://github.com/valnoxy/dive/releases).
2. Use Rufus to extract the Dive ISO to your USB flash drive.
3. Create a new folder called "WIMs" on your USB flash drive.
4. Copy your WIM-Files to the "WIMs" folder. You can extract the WIM file from every Windows iso between Windows Vista and higher. The wim file is located in ```Windows ISO > sources > install.wim```.

> **Note** In some cases, you will only find a ESD file instead. In that case, you'll need to convert the ESD file to a WIM file. You can follow [this guide](https://community.spiceworks.com/how_to/163540-convert-esd-to-wim) to do so.

## Screenshots
![Windows PE with Dive](https://dl.exploitox.de/other/dive-screenshot1.png)

## ℹ️ Disclaimer
> This application will modify the system. I won't be responsible for any damage you've done yourself trying to use this application.

## ⚙️ Compiling the source code
For compiling, you'll need ```Visual Studio 2022``` and ```.NET 6.0```.
Clone this source and restore the NUGET Packages.

## 🧾 License
Dive is licensed under [GNU GENERAL PUBLIC LICENSE](https://github.com/valnoxy/dive/blob/main/LICENSE). So you are allowed to use freely and modify the application. I will not be responsible for any outcome. Proceed with any action at your own risk.

<hr>
<h6 align="center">© 2018 - 2023 valnoxy. All Rights Reserved. 
<br>
By Jonas Günner &lt;jonas@exploitox.de&gt;</h6>
<p align="center">
	<a href="https://github.com/valnoxy/Dive/blob/main/LICENSE"><img src="https://img.shields.io/static/v1.svg?style=for-the-badge&label=License&message=GNU%20GENERAL%20PUBLIC%20%20LICENSE&logoColor=d9e0ee&colorA=363a4f&colorB=b7bdf8"/></a>
</p
