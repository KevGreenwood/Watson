<p align="center">
  <img height="150" src="https://github.com/KevGreenwood/Watson/blob/main/Assets/Watson.svg">
  <h1 align="center">Watson</h1>
</p>

Watson is a small WPF utility born from the ashes of my former project **PrometheusActivator / OpenFuryKMS**.

This application does **not activate Windows**. It is a **read-only tool** designed to display the Windows licensing information already present on your system.

---

## What does Watson show?

- OEM Key (from firmware / BIOS, if available)
- Installed License Key
- Backup Product Key
- Default / Generic Key (if applicable)
- Product ID
- Windows version, build and edition
- Activation ID (ActID)

All keys can be **copied to the clipboard** with a single click.

---

## Screenshots
![NO OEM DEMO](/docs/noOEM.png)
![OEM DEMO](/docs/OEM.png)

---

## Credits
License detection and product key decryption are based on the work by Guilherme Lima: https://github.com/guilhermelim/Get-Windows-Product-Key

## Why Watson?
After ending PrometheusActivator / OpenFuryKMS, I wanted a **clean, legal and transparent** tool focused only on **reading and presenting Windows license data**, similar in spirit to tools like ShowKeyPlus, but with my own design and approach.
