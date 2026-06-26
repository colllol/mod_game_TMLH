# GMConsoleMod - In-Game GM Command Console

MelonLoader mod for Thien Menh Lac Hong that provides an in-game GM command console.

## Features

- **50+ GM Commands**: Add gold, items, teleport, spawn monsters, and more
- **Permission Bypass**: Harmony patches to bypass GM permission checks
- **In-Game UI Console**: Press F1 to toggle, auto-complete, command history
- **Reflection-Based Execution**: Attempts to call game methods via reflection

## Installation

### Prerequisites

1. **Install MelonLoader** into your game:
   - Download from [MelonLoader Releases](https://github.com/LavaGang/MelonLoader/releases)
   - Run the installer and select your game executable
   - This will create a `MelonLoader` folder and `Mods` folder in your game directory

2. **Copy DLL references** to project:
   ```
   GMConsoleMod/
   └── MelonLoader/          ← Create this folder
       ├── MelonLoader.dll
       └── 0Harmony.dll
   ```

### Build & Install

```bash
cd GMConsoleMod
dotnet build
```

Output: `bin\Debug\net6.0\GMConsoleMod.dll` (auto-copies to `Mods\` if exists)

## Controls

| Key | Action |
|-----|--------|
| `F1` | Toggle console |
| `Enter` | Execute command |
| `Tab` | Auto-complete |
| `Up/Down` | Navigate suggestions |
| `Escape` | Close console |

## Commands

### Currency
```
#add_gold <amount>
#add_diamond <amount>
#add_bound_gold <amount>
#add_energy <amount>
#add_honor <amount>
#add_contribution <amount>
#add_reputation <amount>
```

### Experience & Level
```
#add_exp <amount>
#add_level <level>
#add_skill_exp <amount>
#add_talent_point <count>
#add_stat_point <count>
```

### Items
```
#add_item <itemId> [count]
#add_equipment <itemId> [enchant]
#add_set_item <setId>
#add_mount <mountId>
#add_pet <petId>
#add_title <titleId>
#add_skill <skillId>
#clear_inventory
```

### Teleportation
```
#teleport <x> <y> <z>
#teleport_npc <npcId>
#teleport_map <mapId>
#teleport_player <playerName>
#teleport_home
#return
```

### Combat
```
#god_mode
#invisible
#kill [targetName]
#kill_all
#instant_kill
#attack_speed <multiplier>
#cast_speed <multiplier>
```

### Spawning
```
#spawn_boss <bossId>
#spawn_mob <mobId> <count>
#spawn_elite <eliteId>
#summon_boss <bossId>
#despawn_all
```

### Buffs
```
#add_buff <buffId> <duration>
#remove_buff <buffId>
#clear_buffs
#buff_all <buffId>
```

### Stats
```
#set_hp <amount>
#set_max_hp <amount>
#set_mp <amount>
#set_max_mp <amount>
#set_str <value>
#set_dex <value>
#set_int <value>
#set_con <value>
```

### Utility
```
#speed <multiplier>
#goto <x> <y> <z>
#announce <message>
#notice <message>
#revive [playerName]
#unstuck
#reload
#help [command]
```

## Building

```bash
dotnet build
```

Output: `bin/Debug/net6.0/GMConsoleMod.dll`

## Requirements

- MelonLoader 0.6.1+ (installed into game)
- .NET 6.0 SDK
- Game directory containing `MelonLoader.dll` and `0Harmony.dll`

## How It Works

1. **Permission Bypass**: Uses HarmonyX to patch game methods like `GMManager.CheckPermission()` to always return true
2. **Reflection Execution**: Tries to find and invoke GM methods in the game assembly
3. **Fallback**: If methods aren't found, commands log a warning but don't crash

## Disclaimer

This mod is for educational purposes. Using GM commands may violate game Terms of Service.
