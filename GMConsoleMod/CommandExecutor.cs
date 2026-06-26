using System;
using System.Collections.Generic;
using Il2CppSystem;
using Il2CppSystem.Reflection;
using UnityEngine;

namespace GMConsoleMod
{
    public static class CommandExecutor
    {
        private static readonly Dictionary<string, Il2CppInvocation> _commandRoutes = new Dictionary<string, Il2CppInvocation>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Il2CppSystem.Object> _singletonCache = new Dictionary<string, Il2CppSystem.Object>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            _commandRoutes.Clear();
            _singletonCache.Clear();

            // Example route registration - these would come from metadata scan or manual mapping
            RegisterRoute("#add_gold", "Assembly-CSharp", "GMManager", "AddGold", new[] { typeof(int) }, CommandRouteType.SingletonMethod);
            RegisterRoute("#add_diamond", "Assembly-CSharp", "GMManager", "AddDiamond", new[] { typeof(int) }, CommandRouteType.SingletonMethod);
            RegisterRoute("#add_item", "Assembly-CSharp", "GMManager", "AddItem", new[] { typeof(int), typeof(int) }, CommandRouteType.SingletonMethod);
            RegisterRoute("#teleport", "Assembly-CSharp", "GMManager", "Teleport", new[] { typeof(float), typeof(float), typeof(float) }, CommandRouteType.SingletonMethod);
            RegisterRoute("#god_mode", "Assembly-CSharp", "GMManager", "ToggleGodMode", Type.EmptyTypes, CommandRouteType.SingletonMethod);
            RegisterRoute("#kill", "Assembly-CSharp", "GMManager", "KillTarget", new[] { typeof(string) }, CommandRouteType.SingletonMethod);
            RegisterRoute("#spawn_boss", "Assembly-CSharp", "GMManager", "SpawnBoss", new[] { typeof(int) }, CommandRouteType.SingletonMethod);
            RegisterRoute("#announce", "Assembly-CSharp", "NetworkManager", "SendAnnouncement", new[] { typeof(string) }, CommandRouteType.NetworkPacket);
            RegisterRoute("#speed", "Assembly-CSharp", "GMManager", "SetSpeed", new[] { typeof(float) }, CommandRouteType.SingletonProperty);
        }

        public static ExecutionResult Execute(string rawInput, GMCommands.ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return ExecutionResult.Fail("Empty command");

            var parts = rawInput.TrimStart('#', '/').Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return ExecutionResult.Fail("Invalid command format");

            var commandName = parts[0].ToLowerInvariant();
            var args = new string[parts.Length - 1];
            if (args.Length > 0)
                Array.Copy(parts, 1, args, args.Length);

            try
            {
                if (!_commandRoutes.TryGetValue(commandName, out var route))
                {
                    logger.Warning($"Unknown command: {commandName}");
                    return ExecutionResult.Fail($"Unknown command: {commandName}");
                }

                if (route.Type == CommandRouteType.NetworkPacket)
                {
                    return SendSpoofedPacket(commandName, args, logger);
                }

                var target = ResolveSingleton(route);
                if (target == null)
                {
                    logger.Warning($"Could not find target object for {commandName}");
                    return ExecutionResult.Fail($"Target not found: {route.TargetTypeName}");
                }

                var method = ResolveMethod(route);
                if (method == null)
                {
                    logger.Warning($"Could not resolve method {route.MethodName} on {route.TargetTypeName}");
                    return ExecutionResult.Fail($"Method not found: {route.MethodName}");
                }

                var il2cppArgs = Il2CppHelper.BoxArguments(ConvertArgs(route, args, logger));
                var result = method.Invoke(target, il2cppArgs);

                logger.Msg($"[IL2CPP] {commandName} executed successfully");
                return ExecutionResult.Success(result);
            }
            catch (Exception ex)
            {
                logger.Error($"[IL2CPP] {commandName} failed: {ex.Message}");
                return ExecutionResult.Fail($"Exception: {ex.Message}");
            }
        }

        private static Il2CppSystem.Object ResolveSingleton(Il2CppInvocation route)
        {
            if (string.IsNullOrEmpty(route.TargetTypeName))
                return null;

            if (_singletonCache.TryGetValue(route.TargetTypeName, out var cached))
                return cached;

            var fullTypeName = $"{route.TargetTypeName}, {route.AssemblyName}";
            var type = Il2CppHelper.ResolveType(fullTypeName);
            if (type == null)
                return null;

            try
            {
                var instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceField != null)
                {
                    var instance = instanceField.GetValue(null);
                    _singletonCache[route.TargetTypeName] = instance;
                    return instance;
                }

                var property = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (property != null)
                {
                    var instance = property.GetValue(null, null);
                    _singletonCache[route.TargetTypeName] = instance;
                    return instance;
                }

                return Il2CppHelper.FindIl2CppObject(fullTypeName);
            }
            catch
            {
                return null;
            }
        }

        private static MethodInfo ResolveMethod(Il2CppInvocation route)
        {
            if (string.IsNullOrEmpty(route.TargetTypeName) || string.IsNullOrEmpty(route.MethodName))
                return null;

            var fullTypeName = $"{route.TargetTypeName}, {route.AssemblyName}";
            var type = Il2CppHelper.ResolveType(fullTypeName);
            if (type == null)
                return null;

            var parameterTypes = route.ParameterManagedTypes;
            return Il2CppHelper.ResolveMethod(type, route.MethodName, parameterTypes);
        }

        private static object[] ConvertArgs(Il2CppInvocation route, string[] args, GMCommands.ILogger logger)
        {
            if (args == null || args.Length == 0)
                return Array.Empty<object>();

            var result = new object[args.Length];
            var expectedTypes = route.ParameterManagedTypes ?? Array.Empty<Type>();

            for (int i = 0; i < args.Length; i++)
            {
                var targetType = i < expectedTypes.Length ? expectedTypes[i] : typeof(string);
                if (!TryConvertManaged(args[i], targetType, out var converted))
                {
                    logger.Warning($"Argument {i} conversion failed: '{args[i]}' -> {targetType.Name}");
                    converted = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                }
                result[i] = converted;
            }

            return result;
        }

        private static bool TryConvertManaged(string input, Type targetType, out object result)
        {
            result = null;
            if (targetType == typeof(string))
            {
                result = input;
                return true;
            }

            if (targetType == typeof(int) && int.TryParse(input, out var i))
            {
                result = i;
                return true;
            }

            if (targetType == typeof(long) && long.TryParse(input, out var l))
            {
                result = l;
                return true;
            }

            if (targetType == typeof(float) && float.TryParse(input, out var f))
            {
                result = f;
                return true;
            }

            if (targetType == typeof(double) && double.TryParse(input, out var d))
            {
                result = d;
                return true;
            }

            if (targetType == typeof(bool) && bool.TryParse(input, out var b))
            {
                result = b;
                return true;
            }

            return false;
        }

        private static ExecutionResult SendSpoofedPacket(string command, string[] args, GMCommands.ILogger logger)
        {
            try
            {
                var networkMgr = Il2CppHelper.FindIl2CppObject("Assembly-CSharp, NetworkManager");
                if (networkMgr == null)
                {
                    logger.Warning("NetworkManager not found - cannot send spoofed packet");
                    return ExecutionResult.Fail("NetworkManager not found");
                }

                // STUB: Replace with actual packet struct for this game
                var packetData = $"GM:{command}:{string.Join(":", args)}";
                var sendMethod = networkMgr.GetType().GetMethod("Send", BindingFlags.Instance | BindingFlags.Public);
                if (sendMethod == null)
                {
                    logger.Warning("NetworkManager.Send not found");
                    return ExecutionResult.Fail("Send method not found");
                }

                // Example: sendMethod.Invoke(networkMgr, new object[] { packetData });
                logger.Msg($"[Network] Would send spoofed packet: {packetData}");
                return ExecutionResult.Success(null);
            }
            catch (Exception ex)
            {
                logger.Error($"[Network] Packet spoof failed: {ex.Message}");
                return ExecutionResult.Fail($"Packet error: {ex.Message}");
            }
        }

        private static void RegisterRoute(string command, string assembly, string targetType, string method, Type[] parameterTypes, CommandRouteType routeType)
        {
            _commandRoutes[command] = new Il2CppInvocation
            {
                AssemblyName = assembly,
                TargetTypeName = targetType,
                MethodName = method,
                ParameterManagedTypes = parameterTypes ?? Array.Empty<Type>(),
                Type = routeType
            };
        }
    }

    public class ExecutionResult
    {
        public bool Success { get; }
        public string Error { get; }
        public object ReturnValue { get; }

        private ExecutionResult(bool success, string error, object ret)
        {
            Success = success;
            Error = error;
            ReturnValue = ret;
        }

        public static ExecutionResult Success(object returnValue = null) => new ExecutionResult(true, null, returnValue);
        public static ExecutionResult Fail(string error) => new ExecutionResult(false, error, null);
    }

    internal class Il2CppInvocation
    {
        public string AssemblyName { get; set; }
        public string TargetTypeName { get; set; }
        public string MethodName { get; set; }
        public Type[] ParameterManagedTypes { get; set; }
        public CommandRouteType Type { get; set; }
    }

    internal enum CommandRouteType
    {
        SingletonMethod,
        SingletonProperty,
        NetworkPacket,
        StaticMethod
    }
}
