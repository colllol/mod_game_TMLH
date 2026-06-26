using System;
using System.Reflection;

namespace GMConsoleMod
{
    public static class ReflectionHelper
    {
        private static readonly Assembly GameAssembly = GetGameAssembly();
        private static readonly Type[] GMManagerTypes = FindGMManagerTypes();
        private static readonly Type[] CommandHandlerTypes = FindCommandHandlerTypes();

        private static Assembly GetGameAssembly()
        {
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.Contains("GameAssembly") ||
                        assembly.FullName.Contains("Assembly-CSharp") ||
                        assembly.GetName().Name == "GameAssembly")
                    {
                        return assembly;
                    }
                }
            }
            catch { }

            return null;
        }

        private static Type[] FindGMManagerTypes()
        {
            if (GameAssembly == null) return Array.Empty<Type>();

            var types = new System.Collections.Generic.List<Type>();

            try
            {
                foreach (var type in GameAssembly.GetTypes())
                {
                    if (type.Name.Contains("GM") && (type.Name.Contains("Manager") || type.Name.Contains("Command")))
                    {
                        types.Add(type);
                    }
                }
            }
            catch { }

            return types.ToArray();
        }

        private static Type[] FindCommandHandlerTypes()
        {
            if (GameAssembly == null) return Array.Empty<Type>();

            var types = new System.Collections.Generic.List<Type>();

            try
            {
                foreach (var type in GameAssembly.GetTypes())
                {
                    if (type.Name.Contains("Command") || type.Name.Contains("Handler") || type.Name.Contains("Chat"))
                    {
                        types.Add(type);
                    }
                }
            }
            catch { }

            return types.ToArray();
        }

        public static object InvokeGMMethod(string methodName, params object[] parameters)
        {
            if (GameAssembly == null) return null;

            // Try GMManager types first
            foreach (var type in GMManagerTypes)
            {
                var result = InvokeMethod(type, methodName, parameters);
                if (result != null) return result;
            }

            // Try command handler types
            foreach (var type in CommandHandlerTypes)
            {
                var result = InvokeMethod(type, methodName, parameters);
                if (result != null) return result;
            }

            return null;
        }

        private static object InvokeMethod(Type type, string methodName, object[] parameters)
        {
            try
            {
                // Try to find static method first
                var method = FindMethod(type, methodName, parameters?.Length ?? 0);
                if (method != null)
                {
                    return method.Invoke(null, parameters);
                }

                // Try instance method (find any object of this type)
                var instance = GetFirstInstance(type);
                if (instance != null)
                {
                    method = FindMethod(type, methodName, parameters?.Length ?? 0);
                    if (method != null)
                    {
                        return method.Invoke(instance, parameters);
                    }
                }
            }
            catch { }

            return null;
        }

        private static MethodInfo FindMethod(Type type, string name, int paramCount)
        {
            try
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (method.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        var @params = method.GetParameters();
                        if (@params.Length == paramCount)
                            return method;
                        if (paramCount == 0 && @params.Length == 0)
                            return method;
                    }
                }
            }
            catch { }

            return null;
        }

        private static object GetFirstInstance(Type type)
        {
            try
            {
                var unityObjectType = typeof(UnityEngine.Object);
                var findObjectsOfType = typeof(UnityEngine.Object).GetMethod("FindObjectsOfType",
                    new[] { typeof(Type) });

                if (findObjectsOfType != null)
                {
                    var objects = findObjectsOfType.Invoke(null, new object[] { type }) as UnityEngine.Object[];
                    if (objects != null && objects.Length > 0)
                        return objects[0];
                }
            }
            catch { }

            return null;
        }

        public static T GetStaticField<T>(string typeName, string fieldName, T defaultValue = default)
        {
            if (GameAssembly == null) return defaultValue;

            try
            {
                var type = GameAssembly.GetType(typeName);
                if (type != null)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (field != null)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }
            catch { }

            return defaultValue;
        }

        public static void SetStaticField<T>(string typeName, string fieldName, T value)
        {
            if (GameAssembly == null) return;

            try
            {
                var type = GameAssembly.GetType(typeName);
                if (type != null)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (field != null)
                    {
                        field.SetValue(null, value);
                    }
                }
            }
            catch { }
        }

        public static object CallMethod(object instance, string typeName, string methodName, params object[] parameters)
        {
            if (GameAssembly == null) return null;

            try
            {
                var type = GameAssembly.GetType(typeName);
                if (type != null)
                {
                    var method = FindMethod(type, methodName, parameters?.Length ?? 0);
                    if (method != null)
                    {
                        return method.Invoke(instance, parameters);
                    }
                }
            }
            catch { }

            return null;
        }

        public static object GetProperty(object instance, string typeName, string propertyName)
        {
            if (GameAssembly == null) return null;

            try
            {
                var type = GameAssembly.GetType(typeName);
                if (type != null)
                {
                    var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    if (property != null)
                    {
                        return property.GetValue(instance);
                    }
                }
            }
            catch { }

            return null;
        }

        public static T GetSingleton<T>() where T : class
        {
            try
            {
                var field = typeof(T).GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (field != null)
                {
                    return field.GetValue(null) as T;
                }

                var property = typeof(T).GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (property != null)
                {
                    return property.GetValue(null) as T;
                }
            }
            catch { }

            return null;
        }
    }
}
