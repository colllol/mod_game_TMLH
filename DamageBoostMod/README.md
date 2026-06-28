# DamageBoost Mod

Increase damage output by 5-10x for Thien Menh Lac Hong.

## Features
- Auto-patches all damage-related methods found in game assemblies
- **F9** - Toggle damage boost ON/OFF
- **F10** - Increase multiplier (+1)
- **F11** - Decrease multiplier (-1)
- Default multiplier: **10x**

## Installation

### Prerequisites
1. Install MelonLoader into your game folder
2. Copy `MelonLoader.dll` and `Mono.Cecil.dll` to `MelonLoader/net6/`

### Build
```powershell
cd DamageBoostMod
dotnet build
```

Output will be copied to `../Mods/DamageBoostMod.dll`

## Files
- `DamageBoostMod.cs` - Main mod source
- `DamageBoostMod.csproj` - Project file

## How It Works
1. Scans all loaded assemblies for methods containing "Damage", "Attack", "Skill" keywords
2. Uses Harmony to patch numeric-return methods with postfix that multiplies result by configurable factor
3. Auto-patches new assemblies as they load

## Configuration
Edit `_damageMultiplier` in source to change default multiplier (default: 10f = 10x damage)
