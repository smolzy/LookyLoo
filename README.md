# LookyLoo

A lightweight Dalamud plugin for FINAL FANTASY XIV that tracks nearby players and lets you know who is targeting you.

## Features

- **Target Tracker**: Highlights nearby players who are currently targeting you.
- **History Log**: Keeps a record of who targeted you with timestamps for when they looked away.
- **Customizable Info**: Toggle displaying Job, Level, Home World, Free Company tag, or Distance.
- **Quick Actions (Right-Click)**:
  - Open subcommands menu
  - Send Tell (pre-fills the chat input)
  - Invite to party
  - Find on map (places a flag pin and opens the map)
  - View Adventurer Plate & Examine
  - View active Moodles (if the Moodles plugin is installed)
- **Minimal UI**: Compact and customizable layout.

## Commands

- `/looky` - Toggle the main window
- `/lookyconfig` - Open configuration settings

## Installation

Add this custom repository URL in Dalamud (`/xlsettings` -> **Experimental** -> **Custom Plugin Repositories**):

```
https://raw.githubusercontent.com/smolzy/LookyLoo/main/pluginmaster.json
```

Then install **LookyLoo** from the Dalamud Plugin Installer (`/xlplugins`).
