using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace GMConsoleMod
{
    public static class PermissionBypass
    {
        private static HarmonyLib.Harmony _harmony;
        private static GMCommands.ILogger _logger;

        public static void PatchAll(GMCommands.ILogger logger)
        {
            _logger = logger;
            _harmony = new HarmonyLib.Harmony("com.gmmod.console");

            logger.Msg("Applying permission bypass patches...");

            int patchedCount = 0;

            patchedCount += PatchGMManagerMethods();
            patchedCount += PatchPlayerMethods();
            patchedCount += PatchNetworkMethods();
            patchedCount += PatchCommandPermissionMethods();

            logger.Msg($"Applied {patchedCount} permission bypass patches");

            if (patchedCount == 0)
            {
                logger.Warning("No game methods found to patch. Commands may not work without server sync.");
                logger.Warning("Some commands can still work client-side only.");
            }
        }

        public static void UnpatchAll()
        {
            try
            {
                _harmony?.UnpatchAll("com.gmmod.console");
                _logger?.Msg("Unpatched all Harmony patches");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error unpatching: {ex.Message}");
            }
        }

        private static int PatchGMManagerMethods()
        {
            int count = 0;

            string[] gmManagerTypes = new[]
            {
                "GMManager",
                "GMCommandManager",
                "GMCommands",
                "CheatManager",
                "DebugManager",
                "AdminCommands",
                "GameMasterCommands"
            };

            foreach (string typeName in gmManagerTypes)
            {
                count += PatchTypeMethods(typeName);
            }

            return count;
        }

        private static int PatchPlayerMethods()
        {
            int count = 0;

            string[] playerTypes = new[]
            {
                "Player",
                "LocalPlayer",
                "GamePlayer",
                "Character",
                "GameCharacter"
            };

            string[] permissionMethods = new[]
            {
                "IsGM",
                "IsAdmin",
                "IsGameMaster",
                "HasPermission",
                "CheckPermission",
                "CanUseCommand",
                "IsPrivileged",
                "GetAuthorityLevel"
            };

            foreach (string typeName in playerTypes)
            {
                foreach (string methodName in permissionMethods)
                {
                    if (PatchMethod(typeName, methodName))
                        count++;
                }
            }

            return count;
        }

        private static int PatchNetworkMethods()
        {
            int count = 0;

            string[] networkTypes = new[]
            {
                "NetworkManager",
                "NetClient",
                "GameNetworkManager",
                "NetworkHandler"
            };

            string[] validationMethods = new[]
            {
                "ValidateGMRequest",
                "ValidateCommand",
                "CheckServerPermission",
                "VerifyAdminCommand",
                "IsValidGMPacket"
            };

            foreach (string typeName in networkTypes)
            {
                foreach (string methodName in validationMethods)
                {
                    if (PatchMethod(typeName, methodName))
                        count++;
                }
            }

            if (PatchMethod("NetworkManager", "SendGMPacket"))
                count++;
            if (PatchMethod("NetworkManager", "SendGMCommand"))
                count++;

            return count;
        }

        private static int PatchCommandPermissionMethods()
        {
            int count = 0;

            string[] commandTypes = new[]
            {
                "CommandHandler",
                "ChatCommandHandler",
                "ChatManager"
            };

            string[] commandMethods = new[]
            {
                "ProcessCommand",
                "ExecuteCommand",
                "HandleChatCommand",
                "OnChatMessage",
                "TryExecuteCommand"
            };

            foreach (string typeName in commandTypes)
            {
                foreach (string methodName in commandMethods)
                {
                    if (PatchMethod(typeName, methodName))
                        count++;
                }
            }

            return count;
        }

        private static int PatchTypeMethods(string typeName)
        {
            int count = 0;

            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                            foreach (var method in methods)
                            {
                                if (ShouldPatchMethod(method))
                                {
                                    if (PatchSpecificMethod(method))
                                        count++;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                _logger?.Warning($"Error patching type {typeName}: {ex.Message}");
            }

            return count;
        }

        private static bool ShouldPatchMethod(MethodInfo method)
        {
            if (method == null) return false;

            string name = method.Name.ToLower();

            if (name.Contains("permission") || name.Contains("check") || name.Contains("validate") || name.Contains("verify"))
            {
                if (!name.Contains("gm") && !name.Contains("admin") && !name.Contains("cheat"))
                    return false;
            }

            if (name.Contains("isgm") || name.Contains("isadmin") || name.Contains("isgame") || name.Contains("isprivileged"))
            {
                return true;
            }

            return false;
        }

        private static bool PatchMethod(string typeName, string methodName)
        {
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            var method = type.GetMethod(methodName,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                            if (method != null)
                            {
                                return PatchSpecificMethod(method);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                _logger?.Warning($"Could not find method {typeName}.{methodName}: {ex.Message}");
            }

            return false;
        }

        private static bool PatchSpecificMethod(MethodInfo method)
        {
            try
            {
                var patchType = typeof(PermissionBypass);
                var originalMethod = method;

                if (method.ReturnType == typeof(bool))
                {
                    var prefix = AccessTools.Method(patchType, "BoolAlwaysTruePrefix");
                    if (prefix != null)
                    {
                        _harmony.Patch(originalMethod, prefix: new HarmonyMethod(prefix));
                        _logger?.Msg($"Patched: {method.DeclaringType?.Name}.{method.Name}");
                        return true;
                    }
                }
                else if (method.ReturnType == typeof(int) || method.ReturnType == typeof(byte))
                {
                    var prefix = AccessTools.Method(patchType, "IntMaxPrefix");
                    if (prefix != null)
                    {
                        var transpiler = AccessTools.Method(patchType, "IntMaxTranspiler");
                        _harmony.Patch(originalMethod, prefix: new HarmonyMethod(prefix), transpiler: transpiler != null ? new HarmonyMethod(transpiler) : null);
                        _logger?.Msg($"Patched: {method.DeclaringType?.Name}.{method.Name}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Warning($"Error patching {method.Name}: {ex.Message}");
            }

            return false;
        }

        #region Harmony Patches

        public static bool BoolAlwaysTruePrefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        public static bool IntMaxPrefix(ref int __result)
        {
            __result = 255;
            return false;
        }

        public static bool IntMaxPrefix(ref byte __result)
        {
            __result = 255;
            return false;
        }

        public static void BoolAlwaysTruePostfix(ref bool __result)
        {
            __result = true;
        }

        #endregion
    }
}
