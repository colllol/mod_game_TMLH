using System;
using System.Collections.Generic;
using System.Reflection;

namespace GMConsoleMod
{
    public static class GMCommands
    {
        public interface ILogger
        {
            void Msg(object msg);
            void Warning(object msg);
            void Error(object msg);
        }

        private static readonly Dictionary<string, CommandInfo> Commands = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // === Currency & Resources ===
            { "#add_gold", new CommandInfo("add_gold", "<amount>", "Add gold currency", CommandCategory.Currency) },
            { "#add_diamond", new CommandInfo("add_diamond", "<amount>", "Add diamond currency", CommandCategory.Currency) },
            { "#add_bound_gold", new CommandInfo("add_bound_gold", "<amount>", "Add bound gold", CommandCategory.Currency) },
            { "#add_energy", new CommandInfo("add_energy", "<amount>", "Add energy points", CommandCategory.Currency) },
            { "#add_honor", new CommandInfo("add_honor", "<amount>", "Add honor points", CommandCategory.Currency) },
            { "#add_contribution", new CommandInfo("add_contribution", "<amount>", "Add guild contribution", CommandCategory.Currency) },
            { "#add_reputation", new CommandInfo("add_reputation", "<amount>", "Add faction reputation", CommandCategory.Currency) },

            // === Experience & Level ===
            { "#add_exp", new CommandInfo("add_exp", "<amount>", "Add experience points", CommandCategory.Exp) },
            { "#add_level", new CommandInfo("add_level", "<level>", "Set player level", CommandCategory.Exp) },
            { "#add_skill_exp", new CommandInfo("add_skill_exp", "<amount>", "Add skill experience", CommandCategory.Exp) },
            { "#add_talent_point", new CommandInfo("add_talent_point", "<count>", "Add talent points", CommandCategory.Exp) },
            { "#add_stat_point", new CommandInfo("add_stat_point", "<count>", "Add stat points", CommandCategory.Exp) },

            // === Items ===
            { "#add_item", new CommandInfo("add_item", "<itemId> <count>", "Add item by ID", CommandCategory.Items) },
            { "#add_equipment", new CommandInfo("add_equipment", "<itemId> <enchant>", "Add equipment item", CommandCategory.Items) },
            { "#add_set_item", new CommandInfo("add_set_item", "<setId>", "Add a complete equipment set", CommandCategory.Items) },
            { "#add_mount", new CommandInfo("add_mount", "<mountId>", "Add mount by ID", CommandCategory.Items) },
            { "#add_pet", new CommandInfo("add_pet", "<petId>", "Add pet by ID", CommandCategory.Items) },
            { "#add_title", new CommandInfo("add_title", "<titleId>", "Add title by ID", CommandCategory.Items) },
            { "#add_skill", new CommandInfo("add_skill", "<skillId>", "Add skill by ID", CommandCategory.Items) },
            { "#add_guild_skill", new CommandInfo("add_guild_skill", "<skillId>", "Add guild skill", CommandCategory.Items) },
            { "#clear_inventory", new CommandInfo("clear_inventory", "", "Clear all inventory items", CommandCategory.Items) },

            // === Teleportation ===
            { "#teleport", new CommandInfo("teleport", "<x> <y> <z>", "Teleport to coordinates", CommandCategory.Teleport) },
            { "#teleport_npc", new CommandInfo("teleport_npc", "<npcId>", "Teleport to NPC", CommandCategory.Teleport) },
            { "#teleport_map", new CommandInfo("teleport_map", "<mapId>", "Teleport to map/zone", CommandCategory.Teleport) },
            { "#teleport_player", new CommandInfo("teleport_player", "<playerName>", "Teleport to player", CommandCategory.Teleport) },
            { "#teleport_home", new CommandInfo("teleport_home", "", "Teleport to home location", CommandCategory.Teleport) },
            { "#return", new CommandInfo("return", "", "Return to last location", CommandCategory.Teleport) },

            // === Combat ===
            { "#god_mode", new CommandInfo("god_mode", "", "Toggle invincibility", CommandCategory.Combat) },
            { "#invisible", new CommandInfo("invisible", "", "Toggle invisibility", CommandCategory.Combat) },
            { "#kill", new CommandInfo("kill", "[targetName]", "Kill target or self", CommandCategory.Combat) },
            { "#kill_all", new CommandInfo("kill_all", "", "Kill all monsters in range", CommandCategory.Combat) },
            { "#instant_kill", new CommandInfo("instant_kill", "", "Toggle instant kill mode", CommandCategory.Combat) },
            { "#attack_speed", new CommandInfo("attack_speed", "<multiplier>", "Set attack speed multiplier", CommandCategory.Combat) },
            { "#cast_speed", new CommandInfo("cast_speed", "<multiplier>", "Set casting speed multiplier", CommandCategory.Combat) },

            // === Spawning ===
            { "#spawn_boss", new CommandInfo("spawn_boss", "<bossId>", "Spawn a boss monster", CommandCategory.Spawn) },
            { "#spawn_mob", new CommandInfo("spawn_mob", "<mobId> <count>", "Spawn monsters", CommandCategory.Spawn) },
            { "#spawn_elite", new CommandInfo("spawn_elite", "<eliteId>", "Spawn elite monster", CommandCategory.Spawn) },
            { "#summon_boss", new CommandInfo("summon_boss", "<bossId>", "Summon boss at player", CommandCategory.Spawn) },
            { "#despawn_all", new CommandInfo("despawn_all", "", "Despawn all spawned monsters", CommandCategory.Spawn) },

            // === Buffs ===
            { "#add_buff", new CommandInfo("add_buff", "<buffId> <duration>", "Add buff effect", CommandCategory.Buff) },
            { "#remove_buff", new CommandInfo("remove_buff", "<buffId>", "Remove buff effect", CommandCategory.Buff) },
            { "#clear_buffs", new CommandInfo("clear_buffs", "", "Clear all buffs", CommandCategory.Buff) },
            { "#buff_all", new CommandInfo("buff_all", "<buffId>", "Buff all nearby players", CommandCategory.Buff) },

            // === Stats ===
            { "#set_hp", new CommandInfo("set_hp", "<amount>", "Set current HP", CommandCategory.Stats) },
            { "#set_max_hp", new CommandInfo("set_max_hp", "<amount>", "Set max HP", CommandCategory.Stats) },
            { "#set_mp", new CommandInfo("set_mp", "<amount>", "Set current MP", CommandCategory.Stats) },
            { "#set_max_mp", new CommandInfo("set_max_mp", "<amount>", "Set max MP", CommandCategory.Stats) },
            { "#set_str", new CommandInfo("set_str", "<value>", "Set strength", CommandCategory.Stats) },
            { "#set_dex", new CommandInfo("set_dex", "<value>", "Set dexterity", CommandCategory.Stats) },
            { "#set_int", new CommandInfo("set_int", "<value>", "Set intelligence", CommandCategory.Stats) },
            { "#set_con", new CommandInfo("set_con", "<value>", "Set constitution", CommandCategory.Stats) },

            // === Utility ===
            { "#speed", new CommandInfo("speed", "<multiplier>", "Set movement speed", CommandCategory.Utility) },
            { "#goto", new CommandInfo("goto", "<x> <y> <z>", "Go to coordinates", CommandCategory.Utility) },
            { "#announce", new CommandInfo("announce", "<message>", "Send server announcement", CommandCategory.Utility) },
            { "#notice", new CommandInfo("notice", "<message>", "Send notice to player", CommandCategory.Utility) },
            { "#revive", new CommandInfo("revive", "[playerName]", "Revive self or player", CommandCategory.Utility) },
            { "#unstuck", new CommandInfo("unstuck", "", "Teleport to safe location", CommandCategory.Utility) },
            { "#reload", new CommandInfo("reload", "", "Reload mod configuration", CommandCategory.Utility) },
            { "#help", new CommandInfo("help", "[command]", "Show help information", CommandCategory.Utility) },
            { "#list", new CommandInfo("list", "[category]", "List commands by category", CommandCategory.Utility) },
        };

        public static void Initialize(ILogger logger)
        {
            logger.Msg("GMCommands initialized");
            logger.Msg($"Loaded {Commands.Count} commands");
        }

        public static void Execute(string input, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.TrimStart('#').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string commandName = "#" + parts[0].ToLower();
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (!Commands.TryGetValue(commandName, out var cmdInfo))
            {
                logger.Warning($"Unknown command: {commandName}. Type #help for available commands.");
                return;
            }

            try
            {
                ExecuteCommand(cmdInfo.Name, args, logger);
                logger.Msg($"Executed: {commandName}");
            }
            catch (Exception ex)
            {
                logger.Error($"Command failed: {ex.Message}");
            }
        }

        private static void ExecuteCommand(string command, string[] args, ILogger logger)
        {
            switch (command)
            {
                case "help":
                    ShowHelp(args.Length > 0 ? args[0] : null, logger);
                    break;
                case "list":
                    ListCommands(args.Length > 0 ? args[0] : null, logger);
                    break;
                case "add_gold":
                    AddGold(args, logger);
                    break;
                case "add_diamond":
                    AddCurrency("Diamonds", args, logger);
                    break;
                case "add_exp":
                    AddExp(args, logger);
                    break;
                case "add_level":
                    AddLevel(args, logger);
                    break;
                case "add_item":
                    AddItem(args, logger);
                    break;
                case "teleport":
                    Teleport(args, logger);
                    break;
                case "speed":
                    SetSpeed(args, logger);
                    break;
                case "god_mode":
                    ToggleGodMode(logger);
                    break;
                case "invisible":
                    ToggleInvisible(logger);
                    break;
                case "kill":
                    Kill(args, logger);
                    break;
                case "kill_all":
                    KillAll(logger);
                    break;
                case "spawn_mob":
                    SpawnMob(args, logger);
                    break;
                case "spawn_boss":
                    SpawnBoss(args, logger);
                    break;
                case "add_buff":
                    AddBuff(args, logger);
                    break;
                case "clear_buffs":
                    ClearBuffs(logger);
                    break;
                case "set_hp":
                    SetStat("HP", args, logger);
                    break;
                case "set_max_hp":
                    SetMaxStat("HP", args, logger);
                    break;
                case "set_mp":
                    SetStat("MP", args, logger);
                    break;
                case "set_max_mp":
                    SetMaxStat("MP", args, logger);
                    break;
                case "set_str":
                    SetAttribute("STR", args, logger);
                    break;
                case "set_dex":
                    SetAttribute("DEX", args, logger);
                    break;
                case "set_int":
                    SetAttribute("INT", args, logger);
                    break;
                case "set_con":
                    SetAttribute("CON", args, logger);
                    break;
                case "announce":
                    Announce(args, logger);
                    break;
                case "notice":
                    Notice(args, logger);
                    break;
                case "unstuck":
                    Unstuck(logger);
                    break;
                case "reload":
                    Reload(logger);
                    break;
                case "goto":
                    Teleport(args, logger);
                    break;
                case "add_bound_gold":
                    AddCurrency("BoundGold", args, logger);
                    break;
                case "add_energy":
                    AddCurrency("Energy", args, logger);
                    break;
                case "add_honor":
                    AddCurrency("Honor", args, logger);
                    break;
                case "add_contribution":
                    AddCurrency("Contribution", args, logger);
                    break;
                case "add_reputation":
                    AddCurrency("Reputation", args, logger);
                    break;
                case "add_skill_exp":
                    AddCurrency("SkillExp", args, logger);
                    break;
                case "add_talent_point":
                    AddPoints("Talent", args, logger);
                    break;
                case "add_stat_point":
                    AddPoints("Stat", args, logger);
                    break;
                case "add_equipment":
                    AddEquipment(args, logger);
                    break;
                case "add_set_item":
                    AddSetItem(args, logger);
                    break;
                case "add_mount":
                    AddMount(args, logger);
                    break;
                case "add_pet":
                    AddPet(args, logger);
                    break;
                case "add_title":
                    AddTitle(args, logger);
                    break;
                case "add_skill":
                    AddSkill(args, logger);
                    break;
                case "add_guild_skill":
                    AddGuildSkill(args, logger);
                    break;
                case "clear_inventory":
                    ClearInventory(logger);
                    break;
                case "teleport_npc":
                    TeleportToNPC(args, logger);
                    break;
                case "teleport_map":
                    TeleportToMap(args, logger);
                    break;
                case "teleport_player":
                    TeleportToPlayer(args, logger);
                    break;
                case "teleport_home":
                    TeleportHome(logger);
                    break;
                case "return":
                    Return(logger);
                    break;
                case "instant_kill":
                    ToggleInstantKill(logger);
                    break;
                case "attack_speed":
                    SetAttackSpeed(args, logger);
                    break;
                case "cast_speed":
                    SetCastSpeed(args, logger);
                    break;
                case "spawn_elite":
                    SpawnElite(args, logger);
                    break;
                case "summon_boss":
                    SummonBoss(args, logger);
                    break;
                case "despawn_all":
                    DespawnAll(logger);
                    break;
                case "remove_buff":
                    RemoveBuff(args, logger);
                    break;
                case "buff_all":
                    BuffAll(args, logger);
                    break;
                case "revive":
                    Revive(args, logger);
                    break;
                default:
                    logger.Warning($"Command '{command}' not yet implemented");
                    break;
            }
        }

        private static void ShowHelp(string command, ILogger logger)
        {
            if (string.IsNullOrEmpty(command))
            {
                logger.Msg("=== GM Console Help ===");
                logger.Msg("Type #help <command> for details");
                logger.Msg("Type #list [category] to list commands");
                logger.Msg("");
                logger.Msg("Categories: Currency, Exp, Items, Teleport, Combat, Spawn, Buff, Stats, Utility");
            }
            else if (Commands.TryGetValue("#" + command, out var cmd))
            {
                logger.Msg($"=== #{cmd.Name} ===");
                logger.Msg($"Usage: #{cmd.Name} {cmd.Usage}");
                logger.Msg($"Description: {cmd.Description}");
                logger.Msg($"Category: {cmd.Category}");
            }
            else
            {
                logger.Warning($"Unknown command: {command}");
            }
        }

        private static void ListCommands(string category, ILogger logger)
        {
            if (string.IsNullOrEmpty(category))
            {
                ShowHelp(null, logger);
                return;
            }

            if (Enum.TryParse<CommandCategory>(category, true, out var cat))
            {
                logger.Msg($"=== {cat} Commands ===");
                foreach (var kvp in Commands)
                {
                    if (kvp.Value.Category == cat)
                    {
                        logger.Msg($"  #{kvp.Value.Name} {kvp.Value.Usage}");
                    }
                }
            }
            else
            {
                logger.Msg($"=== Commands matching '{category}' ===");
                foreach (var kvp in Commands)
                {
                    if (kvp.Key.Contains(category, StringComparison.OrdinalIgnoreCase) ||
                        kvp.Value.Name.Contains(category, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Msg($"  #{kvp.Value.Name} {kvp.Value.Usage}");
                    }
                }
            }
        }

        private static void AddGold(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !long.TryParse(args[0], out var amount))
            {
                logger.Warning("Usage: #add_gold <amount>");
                return;
            }
            logger.Msg($"Adding {amount} gold...");
        }

        private static void AddCurrency(string type, string[] args, ILogger logger)
        {
            if (args.Length < 1 || !long.TryParse(args[0], out var amount))
            {
                logger.Warning($"Usage: #add_{type.ToLower()} <amount>");
                return;
            }
            logger.Msg($"Adding {amount} {type}...");
        }

        private static void AddExp(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !long.TryParse(args[0], out var amount))
            {
                logger.Warning("Usage: #add_exp <amount>");
                return;
            }
            logger.Msg($"Adding {amount} experience...");
        }

        private static void AddLevel(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var level))
            {
                logger.Warning("Usage: #add_level <level>");
                return;
            }
            logger.Msg($"Setting level to {level}...");
        }

        private static void AddItem(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var itemId))
            {
                logger.Warning("Usage: #add_item <itemId> [count]");
                return;
            }
            var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 1;
            logger.Msg($"Adding item {itemId} x{count}...");
        }

        private static void AddEquipment(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var itemId))
            {
                logger.Warning("Usage: #add_equipment <itemId> [enchant]");
                return;
            }
            var enchant = args.Length > 1 && int.TryParse(args[1], out var e) ? e : 0;
            logger.Msg($"Adding equipment {itemId} +{enchant}...");
        }

        private static void AddSetItem(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var setId))
            {
                logger.Warning("Usage: #add_set_item <setId>");
                return;
            }
            logger.Msg($"Adding equipment set {setId}...");
        }

        private static void AddMount(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var mountId))
            {
                logger.Warning("Usage: #add_mount <mountId>");
                return;
            }
            logger.Msg($"Adding mount {mountId}...");
        }

        private static void AddPet(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var petId))
            {
                logger.Warning("Usage: #add_pet <petId>");
                return;
            }
            logger.Msg($"Adding pet {petId}...");
        }

        private static void AddTitle(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var titleId))
            {
                logger.Warning("Usage: #add_title <titleId>");
                return;
            }
            logger.Msg($"Adding title {titleId}...");
        }

        private static void AddSkill(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var skillId))
            {
                logger.Warning("Usage: #add_skill <skillId>");
                return;
            }
            logger.Msg($"Adding skill {skillId}...");
        }

        private static void AddGuildSkill(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var skillId))
            {
                logger.Warning("Usage: #add_guild_skill <skillId>");
                return;
            }
            logger.Msg($"Adding guild skill {skillId}...");
        }

        private static void ClearInventory(ILogger logger)
        {
            logger.Msg("Clearing inventory...");
        }

        private static void Teleport(string[] args, ILogger logger)
        {
            if (args.Length < 3 ||
                !float.TryParse(args[0], out var x) ||
                !float.TryParse(args[1], out var y) ||
                !float.TryParse(args[2], out var z))
            {
                logger.Warning("Usage: #teleport <x> <y> <z>");
                return;
            }
            logger.Msg($"Teleporting to ({x}, {y}, {z})...");
        }

        private static void TeleportToNPC(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var npcId))
            {
                logger.Warning("Usage: #teleport_npc <npcId>");
                return;
            }
            logger.Msg($"Teleporting to NPC {npcId}...");
        }

        private static void TeleportToMap(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var mapId))
            {
                logger.Warning("Usage: #teleport_map <mapId>");
                return;
            }
            logger.Msg($"Teleporting to map {mapId}...");
        }

        private static void TeleportToPlayer(string[] args, ILogger logger)
        {
            if (args.Length < 1)
            {
                logger.Warning("Usage: #teleport_player <playerName>");
                return;
            }
            logger.Msg($"Teleporting to player {args[0]}...");
        }

        private static void TeleportHome(ILogger logger)
        {
            logger.Msg("Teleporting home...");
        }

        private static void Return(ILogger logger)
        {
            logger.Msg("Returning to last location...");
        }

        private static void SetSpeed(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out var speed))
            {
                logger.Warning("Usage: #speed <multiplier>");
                return;
            }
            logger.Msg($"Setting speed to {speed}x...");
        }

        private static void ToggleGodMode(ILogger logger)
        {
            logger.Msg("Toggling god mode...");
        }

        private static void ToggleInvisible(ILogger logger)
        {
            logger.Msg("Toggling invisibility...");
        }

        private static void ToggleInstantKill(ILogger logger)
        {
            logger.Msg("Toggling instant kill mode...");
        }

        private static void SetAttackSpeed(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out var speed))
            {
                logger.Warning("Usage: #attack_speed <multiplier>");
                return;
            }
            logger.Msg($"Setting attack speed to {speed}x...");
        }

        private static void SetCastSpeed(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out var speed))
            {
                logger.Warning("Usage: #cast_speed <multiplier>");
                return;
            }
            logger.Msg($"Setting cast speed to {speed}x...");
        }

        private static void Kill(string[] args, ILogger logger)
        {
            var target = args.Length > 0 ? args[0] : "self";
            logger.Msg($"Killing {target}...");
        }

        private static void KillAll(ILogger logger)
        {
            logger.Msg("Killing all monsters...");
        }

        private static void SpawnMob(string[] args, ILogger logger)
        {
            if (args.Length < 2 ||
                !int.TryParse(args[0], out var mobId) ||
                !int.TryParse(args[1], out var count))
            {
                logger.Warning("Usage: #spawn_mob <mobId> <count>");
                return;
            }
            logger.Msg($"Spawning {count}x mob {mobId}...");
        }

        private static void SpawnBoss(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var bossId))
            {
                logger.Warning("Usage: #spawn_boss <bossId>");
                return;
            }
            logger.Msg($"Spawning boss {bossId}...");
        }

        private static void SpawnElite(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var eliteId))
            {
                logger.Warning("Usage: #spawn_elite <eliteId>");
                return;
            }
            logger.Msg($"Spawning elite {eliteId}...");
        }

        private static void SummonBoss(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var bossId))
            {
                logger.Warning("Usage: #summon_boss <bossId>");
                return;
            }
            logger.Msg($"Summoning boss {bossId} at player...");
        }

        private static void DespawnAll(ILogger logger)
        {
            logger.Msg("Despawning all monsters...");
        }

        private static void AddBuff(string[] args, ILogger logger)
        {
            if (args.Length < 2 ||
                !int.TryParse(args[0], out var buffId) ||
                !int.TryParse(args[1], out var duration))
            {
                logger.Warning("Usage: #add_buff <buffId> <duration>");
                return;
            }
            logger.Msg($"Adding buff {buffId} for {duration}s...");
        }

        private static void RemoveBuff(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var buffId))
            {
                logger.Warning("Usage: #remove_buff <buffId>");
                return;
            }
            logger.Msg($"Removing buff {buffId}...");
        }

        private static void ClearBuffs(ILogger logger)
        {
            logger.Msg("Clearing all buffs...");
        }

        private static void BuffAll(string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var buffId))
            {
                logger.Warning("Usage: #buff_all <buffId>");
                return;
            }
            logger.Msg($"Buffing all nearby players with {buffId}...");
        }

        private static void SetStat(string stat, string[] args, ILogger logger)
        {
            if (args.Length < 1 || !long.TryParse(args[0], out var amount))
            {
                logger.Warning($"Usage: #set_{stat.ToLower()} <amount>");
                return;
            }
            logger.Msg($"Setting {stat} to {amount}...");
        }

        private static void SetMaxStat(string stat, string[] args, ILogger logger)
        {
            if (args.Length < 1 || !long.TryParse(args[0], out var amount))
            {
                logger.Warning($"Usage: #set_max_{stat.ToLower()} <amount>");
                return;
            }
            logger.Msg($"Setting max {stat} to {amount}...");
        }

        private static void SetAttribute(string attr, string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var value))
            {
                logger.Warning($"Usage: #set_{attr.ToLower()} <value>");
                return;
            }
            logger.Msg($"Setting {attr} to {value}...");
        }

        private static void AddPoints(string type, string[] args, ILogger logger)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var count))
            {
                logger.Warning($"Usage: #add_{type.ToLower()}_point <count>");
                return;
            }
            logger.Msg($"Adding {count} {type.ToLower()} points...");
        }

        private static void Announce(string[] args, ILogger logger)
        {
            if (args.Length < 1)
            {
                logger.Warning("Usage: #announce <message>");
                return;
            }
            logger.Msg($"Announcement: {string.Join(" ", args)}");
        }

        private static void Notice(string[] args, ILogger logger)
        {
            if (args.Length < 1)
            {
                logger.Warning("Usage: #notice <message>");
                return;
            }
            logger.Msg($"Notice: {string.Join(" ", args)}");
        }

        private static void Unstuck(ILogger logger)
        {
            logger.Msg("Teleporting to safe location...");
        }

        private static void Reload(ILogger logger)
        {
            logger.Msg("Reloading configuration...");
        }

        private static void Revive(string[] args, ILogger logger)
        {
            var target = args.Length > 0 ? args[0] : "self";
            logger.Msg($"Reviving {target}...");
        }

        public static List<string> GetSuggestions(string partial)
        {
            var suggestions = new List<string>();
            if (string.IsNullOrEmpty(partial)) return suggestions;

            foreach (var kvp in Commands)
            {
                if (kvp.Key.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(kvp.Key);
                }
            }

            return suggestions;
        }

        public static bool CommandExists(string command)
        {
            return Commands.ContainsKey(command.StartsWith("#") ? command : "#" + command);
        }

        public static CommandInfo GetCommandInfo(string command)
        {
            Commands.TryGetValue(command.StartsWith("#") ? command : "#" + command, out var info);
            return info;
        }
    }

    public enum CommandCategory
    {
        Currency,
        Exp,
        Items,
        Teleport,
        Combat,
        Spawn,
        Buff,
        Stats,
        Utility
    }

    public class CommandInfo
    {
        public string Name { get; }
        public string Usage { get; }
        public string Description { get; }
        public CommandCategory Category { get; }

        public CommandInfo(string name, string usage, string description, CommandCategory category)
        {
            Name = name;
            Usage = usage;
            Description = description;
            Category = category;
        }
    }
}
