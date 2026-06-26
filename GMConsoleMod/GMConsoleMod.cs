using MelonLoader;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(GMConsoleMod.GMConsoleMod), "GMConsoleMod", "1.0.0", "Modder", "In-game GM Command Console for Thien Menh Lac Hong")]
[assembly: MelonGame("Unity", "ThienMenhLacHong")]

namespace GMConsoleMod
{
    public class GMConsoleMod : MelonMod
    {
        public static GMConsoleMod Instance { get; private set; }
        public static readonly List<string> CommandHistory = new List<string>();
        public static int HistoryIndex = -1;

        private ConsoleUI _consoleUI;
        private bool _consoleOpen = false;
        private bool _f1Pressed = false;
        private static Action<object> _logAction;
        private static Action<object> _warnAction;
        private static Action<object> _errorAction;

        public override void OnInitializeMelon()
        {
            Instance = this;
            LoggerInstance.Msg("=== GMConsoleMod v1.0.0 ===");
            LoggerInstance.Msg("Loading...");
            
            _logAction = LoggerInstance.Msg;
            _warnAction = LoggerInstance.Warning;
            _errorAction = LoggerInstance.Error;

            try
            {
                PermissionBypass.PatchAll(new LoggerWrapper(_logAction, _warnAction, _errorAction));
                LoggerInstance.Msg("PermissionBypass patched");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"PermissionBypass error: {ex.Message}");
            }

            GMCommands.Initialize(new LoggerWrapper(_logAction, _warnAction, _errorAction));
            LoggerInstance.Msg("GMCommands initialized");
            LoggerInstance.Msg("READY - Press F1 to toggle console");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene loaded: {sceneName} (index {buildIndex})");
            if (buildIndex > 0)
            {
                LoggerInstance.Msg("Game scene detected - console should work now");
            }
        }

        public override void OnUpdate()
        {
            // Check F1 with flag to avoid multiple triggers
            if (Input.GetKey(KeyCode.F1) && !_f1Pressed)
            {
                _f1Pressed = true;
                LoggerInstance.Msg("F1 pressed - toggling console");
                ToggleConsole();
            }
            
            if (!Input.GetKey(KeyCode.F1))
            {
                _f1Pressed = false;
            }

            if (_consoleOpen && _consoleUI != null)
            {
                HandleConsoleInput();
            }
        }

        public void ToggleConsole()
        {
            _consoleOpen = !_consoleOpen;
            LoggerInstance.Msg($"Console now: {(_consoleOpen ? "OPEN" : "CLOSED")}");

            if (_consoleOpen)
            {
                if (_consoleUI == null)
                {
                    LoggerInstance.Msg("Creating ConsoleUI...");
                    _consoleUI = new ConsoleUI();
                    LoggerInstance.Msg("ConsoleUI created");
                }
                _consoleUI.Show();
            }
            else
            {
                _consoleUI?.Hide();
            }
        }

        private void HandleConsoleInput()
        {
            // History navigation handled in ConsoleUI now
        }

        public static void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            if (CommandHistory.Count == 0 || CommandHistory[^1] != command)
            {
                CommandHistory.Add(command);
            }
            HistoryIndex = CommandHistory.Count;

            _logAction?.Invoke($"> {command}");
            GMCommands.Execute(command, new LoggerWrapper(_logAction, _warnAction, _errorAction));
        }

        public override void OnGUI()
        {
            // Always render to check if GUI works
            if (_consoleOpen && _consoleUI != null)
            {
                _consoleUI.OnGUI();
            }
            
            // Debug: Show small indicator when mod is loaded
            GUI.Label(new Rect(10, 10, 200, 20), "GMConsoleMod Active");
        }

        public override void OnApplicationQuit()
        {
            PermissionBypass.UnpatchAll();
            LoggerInstance.Msg("GMConsoleMod unloaded");
        }

        private class LoggerWrapper : GMCommands.ILogger
        {
            private readonly Action<object> _log;
            private readonly Action<object> _warn;
            private readonly Action<object> _error;

            public LoggerWrapper(Action<object> log, Action<object> warn, Action<object> error)
            {
                _log = log;
                _warn = warn;
                _error = error;
            }

            public void Msg(object msg) => _log(msg);
            public void Warning(object msg) => _warn(msg);
            public void Error(object msg) => _error(msg);
        }
    }
}
