using System;
using System.Collections.Generic;
using Il2CppSystem;
using Il2CppSystem.Reflection;
using UnityEngine;

namespace GMConsoleMod
{
    public static class Il2CppHelper
    {
        private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);

        public static Type ResolveType(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                return null;

            if (_typeCache.TryGetValue(assemblyQualifiedName, out var cached))
                return cached;

            try
            {
                var type = Type.GetType(assemblyQualifiedName);
                if (type != null)
                {
                    _typeCache[assemblyQualifiedName] = type;
                    return type;
                }
            }
            catch { }

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = asm.GetType(assemblyQualifiedName);
                    if (type != null)
                    {
                        _typeCache[assemblyQualifiedName] = type;
                        return type;
                    }
                }
            }
            catch { }

            return null;
        }

        public static MethodInfo ResolveMethod(Type type, string methodName, Type[] parameterTypes = null)
        {
            if (type == null || string.IsNullOrEmpty(methodName))
                return null;

            var cacheKey = $"{type.FullName}::{methodName}";
            if (_methodCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var method = parameterTypes != null
                    ? type.GetMethod(methodName, flags, null, parameterTypes, null)
                    : type.GetMethod(methodName, flags);

                if (method != null)
                {
                    _methodCache[cacheKey] = method;
                    return method;
                }
            }
            catch { }

            return null;
        }

        public static Il2CppSystem.Object FindIl2CppObject(string className)
        {
            var type = ResolveType(className);
            if (type == null)
                return null;

            try
            {
                return UnityEngine.Object.FindObjectOfType(type);
            }
            catch
            {
                return null;
            }
        }

        public static Il2CppSystem.Object[] BoxArguments(object[] managedArgs)
        {
            if (managedArgs == null || managedArgs.Length == 0)
                return Array.Empty<Il2CppSystem.Object>();

            var il2cppArgs = new Il2CppSystem.Object[managedArgs.Length];
            for (int i = 0; i < managedArgs.Length; i++)
            {
                il2cppArgs[i] = BoxManagedObject(managedArgs[i]);
            }
            return il2cppArgs;
        }

        public static Il2CppSystem.Object BoxManagedObject(object obj)
        {
            if (obj == null)
                return null;

            switch (obj)
            {
                case Il2CppSystem.Object il2cppObj:
                    return il2cppObj;
                case string str:
                    return new Il2CppSystem.String(str);
                case int i:
                    return Il2CppSystem.Int32.Box(i);
                case long l:
                    return Il2CppSystem.Int64.Box(l);
                case float f:
                    return Il2CppSystem.Single.Box(f);
                case double d:
                    return Il2CppSystem.Double.Box(d);
                case bool b:
                    return Il2CppSystem.Boolean.Box(b);
                case byte b8:
                    return Il2CppSystem.Byte.Box(b8);
                case short s:
                    return Il2CppSystem.Int16.Box(s);
                case uint ui:
                    return Il2CppSystem.UInt32.Box(ui);
                case ulong ul:
                    return Il2CppSystem.UInt64.Box(ul);
                default:
                    try
                    {
                        return obj as Il2CppSystem.Object;
                    }
                    catch
                    {
                        return null;
                    }
            }
        }
    }
}
