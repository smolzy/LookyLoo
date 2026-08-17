<div align="center">

# LookyLoo 🐱👀

**A lightweight, modern targeting radar & player monitor for FINAL FANTASY XIV (Dalamud).**

*Catch players staring at you red-handed.*

---

</div>

## ✨ Features

- 🎯 **Target Detection & Live Monitoring**
  - Instantly highlights any nearby player who is currently targeting your character with a distinctive green indicator.
  - Target or focus-target nearby players with customizable click behaviors (auto-target on click, auto-focus on hover).

- 🕒 **Targeting History & Timestamps**
  - The **"Targeting Me"** tab functions as a live session history.
  - When someone stops targeting you, they remain in the list (grayed out) with the exact time they looked away (e.g. `20:34`).
  - History is preserved for up to 60 minutes even if the player moves out of range.

- 🎛️ **Configurable Display Columns**
  - Toggle and customize what information is shown for each player:
    - **Job Abbreviation** (e.g. `[PLD]`, `[WHM]`, `[SAM]`)
    - **Level** (including Eureka / Bozja Foray levels)
    - **World / Server** (e.g. `@Moogle`, `@Ragnarok`)
    - **Free Company Tag** (e.g. `«TAG»`)
    - **Distance** in yalms/meters (e.g. `14m`)

- 🎭 **Moodles Integration (IPC)**
  - Seamlessly integrates with the **Moodles** plugin via native IPC.
  - Right-click any player with active Moodles to inspect their custom buffs, debuffs, stack counts, and applier names in a dedicated viewer modal.

- ⚡ **Interactive Context Menu (Right-Click Actions)**
  - **Target / Focus Target**
  - **Open Menu** (Native in-game subcommand menu)
  - **Send Tell** (Directly pre-fills `/tell <Name>@<World>` in chat)
  - **Invite to Party** (`/pcmd add`)
  - **Find on Map** (Opens map and drops a flag marker on the player's exact location)
  - **Examine Character**
  - **View Adventurer Plate**
  - **Copy Name**

- 🖥️ **Modern & Compact Interface**
  - Flat, border-less, square design built for minimal screen footprint and high readability.
  - Native Dalamud window integration with automatic open-on-login support.

---

## ⌨️ Commands

| Command | Description |
| :--- | :--- |
| `/looky` | Toggle the main LookyLoo window |
| `/lookyconfig` | Open the LookyLoo configuration menu |

---

## 📦 Installation (Custom Repository)

1. In-game, open Dalamud settings by typing `/xlsettings` in chat.
2. Navigate to the **Experimental** tab.
3. In the **Custom Plugin Repositories** section, add your repository raw JSON URL:
   ```
   https://raw.githubusercontent.com/smolzy/LookyLoo/main/pluginmaster.json
   ```
4. Click **Save and Close**.
5. Open the Plugin Installer (`/xlplugins`), search for **LookyLoo**, and click **Install**!

---

## 🛠️ Building from Source

### Prerequisites
- .NET 8 SDK
- FINAL FANTASY XIV & XIVLauncher with Dalamud

### Build
```bash
git clone https://github.com/smolzy/LookyLoo.git
cd LookyLoo
dotnet build -c Release
```
The compiled plugin package will be located at:
`SamplePlugin/bin/x64/Release/LookyLoo/latest.zip`

---

## 📄 License
This project is licensed under the [AGPL-3.0 License](LICENSE.md).
