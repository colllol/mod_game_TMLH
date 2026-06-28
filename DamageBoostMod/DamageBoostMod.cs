using MelonLoader;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

[assembly: MelonInfo(typeof(DamageBoost.DamageBoostMod), "DamageBoost", "1.0.0", "Modder", "Increase damage output by 5-10x")]
[assembly: MelonGame("Unity", "ThienMenhLacHong")]

namespace DamageBoost
{
    public class DamageBoostMod : MelonMod
    {
        private static DamageBoostMod _instance;
        private HarmonyLib.Harmony _harmony;
        private bool _damageBoostEnabled = true;
        private float _damageMultiplier = 10f;

        public override void OnInitializeMelon()
        {
            _instance = this;
            _harmony = new HarmonyLib.Harmony("com.damageboost.mod");

            LoggerInstance.Msg("=== DamageBoost v1.0.0 ===");
            LoggerInstance.Msg($"Damage Multiplier: {_damageMultiplier}x");

            int patched = 0;

            // Scan and patch damage-related methods
            patched += PatchDamageMethods();
            patched += PatchAttackMethods();
            patched += PatchSkillMethods();

            LoggerInstance.Msg($"Patched {patched} damage methods");
            LoggerInstance.Msg("Damage boost active!");

            // Auto-patch new types when they load
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }

        public override void OnUpdate()
        {
            // Toggle multiplier with F9 (0x78 = F9)
            if (GetAsyncKeyState(0x78) < 0)
            {
                _damageBoostEnabled = !_damageBoostEnabled;
                LoggerInstance.Msg($"Damage boost: {(_damageBoostEnabled ? "ON" : "OFF")}");
                System.Threading.Thread.Sleep(300);
            }

            // Adjust multiplier with F10/F11
            if (GetAsyncKeyState(0x79) < 0) // F10
            {
                _damageMultiplier = Math.Min(_damageMultiplier + 1f, 100f);
                LoggerInstance.Msg($"Multiplier: {_damageMultiplier}x");
                System.Threading.Thread.Sleep(150);
            }
            if (GetAsyncKeyState(0x7A) < 0) // F11
            {
                _damageMultiplier = Math.Max(_damageMultiplier - 1f, 1f);
                LoggerInstance.Msg($"Multiplier: {_damageMultiplier}x");
                System.Threading.Thread.Sleep(150);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (_damageBoostEnabled)
            {
                PatchAssemblyDamageMethods(args.LoadedAssembly);
            }
        }

        private int PatchDamageMethods()
        {
            int count = 0;
            string[] damageKeywords = { "Damage", "DealDamage", "CalculateDamage", "ApplyDamage", "OnHit", "TakeDamage" };

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                        {
                            string name = method.Name.ToLower();
                            foreach (var keyword in damageKeywords)
                            {
                                if (name.Contains(keyword.ToLower()))
                                {
                                    if (method.ReturnType == typeof(float) || method.ReturnType == typeof(int))
                                    {
                                        if (TryPatchDamageMethod(method))
                                            count++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return count;
        }

        private int PatchAttackMethods()
        {
            int count = 0;
            string[] attackKeywords = { "Attack", "DoAttack", "MeleeAttack", "RangeAttack", "CastSkill", "OnSkillCast" };

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                        {
                            string name = method.Name.ToLower();
                            foreach (var keyword in attackKeywords)
                            {
                                if (name.Contains(keyword.ToLower()))
                                {
                                    if (IsNumericReturnType(method))
                                    {
                                        if (TryPatchDamageMethod(method))
                                            count++;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return count;
        }

        private int PatchSkillMethods()
        {
            int count = 0;
            string[] skillKeywords = { "SkillDamage", "SpellDamage", "GetSkillPower", "CalculateSkillDamage" };

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                        {
                            foreach (var keyword in skillKeywords)
                            {
                                if (method.Name.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (IsNumericReturnType(method))
                                    {
                                        if (TryPatchDamageMethod(method))
                                            count++;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return count;
        }

        private void PatchAssemblyDamageMethods(Assembly asm)
        {
            try
            {
                foreach (var type in asm.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                    {
                        string name = method.Name.ToLower();
                        if ((name.Contains("damage") || name.Contains("attack")) && IsNumericReturnType(method))
                        {
                            TryPatchDamageMethod(method);
                        }
                    }
                }
            }
            catch { }
        }

        private bool IsNumericReturnType(MethodInfo method)
        {
            var ret = method.ReturnType;
            return ret == typeof(float) || ret == typeof(int) || ret == typeof(double) || ret == typeof(long);
        }

        private bool TryPatchDamageMethod(MethodInfo method)
        {
            try
            {
                var patchType = typeof(DamageBoostMod);
                string patchId = $"{method.DeclaringType?.Name}.{method.Name}";

                // Skip already patched
                if (_patchedMethods.Contains(patchId))
                    return false;

                var prefix = AccessTools.Method(patchType, "DamagePrefix");
                var postfix = AccessTools.Method(patchType, "DamagePostfix_Float");

                if (method.ReturnType == typeof(float))
                {
                    _harmony.Patch(method, prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                                         postfix: postfix != null ? new HarmonyMethod(postfix) : null);
                    _patchedMethods.Add(patchId);
                    LoggerInstance.Msg($"Patched: {patchId}");
                    return true;
                }

                var postfixInt = AccessTools.Method(patchType, "DamagePostfix_Int");
                if (method.ReturnType == typeof(int))
                {
                    _harmony.Patch(method, prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                                         postfix: postfixInt != null ? new HarmonyMethod(postfixInt) : null);
                    _patchedMethods.Add(patchId);
                    LoggerInstance.Msg($"Patched: {patchId}");
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static readonly HashSet<string> _patchedMethods = new HashSet<string>();

        // ===== HARMONY PATCHES =====

        [HarmonyPrefix]
        public static void DamagePrefix()
        {
            if (!_instance._damageBoostEnabled) return;
        }

        [HarmonyPostfix]
        public static void DamagePostfix_Float(ref float __result)
        {
            if (_instance == null || !_instance._damageBoostEnabled) return;
            __result *= _instance._damageMultiplier;
        }

        [HarmonyPostfix]
        public static void DamagePostfix_Int(ref int __result)
        {
            if (_instance == null || !_instance._damageBoostEnabled) return;
            __result = (int)(__result * _instance._damageMultiplier);
        }

        // Additional patch for methods with out parameters
        [HarmonyPatch]
        public static void DamagePostfix_RefFloat(ref float __result)
        {
            if (_instance == null || !_instance._damageBoostEnabled) return;
            __result *= _instance._damageMultiplier;
        }

        public override void OnApplicationQuit()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            _harmony?.UnpatchAll("com.damageboost.mod");
            LoggerInstance.Msg("DamageBoost unloaded");
        }
    }
}
