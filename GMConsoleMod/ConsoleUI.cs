using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMConsoleMod
{
    public class ConsoleUI
    {
        private bool _isVisible = false;
        private string _inputText = "";
        private Vector2 _scrollPosition = Vector2.zero;
        private readonly List<string> _outputLines = new List<string>();
        private const int MaxOutputLines = 100;

        private Rect _windowRect;
        private const float ConsoleWidth = 500f;
        private const float ConsoleHeight = 350f;

        private Texture2D _backgroundTexture;
        private Texture2D _inputBackgroundTexture;
        private bool _texturesCreated = false;

        public ConsoleUI()
        {
            InitializeWindow();
            CreateTextures();
            AddOutput("=== GM Console ===");
            AddOutput("Press #help for commands");
            AddOutput("");
        }

        private void InitializeWindow()
        {
            float x = Screen.width * 0.25f;
            float y = Screen.height * 0.15f;
            _windowRect = new Rect(x, y, ConsoleWidth, ConsoleHeight);
        }

        private void CreateTextures()
        {
            if (_texturesCreated) return;

            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            _backgroundTexture.Apply();

            _inputBackgroundTexture = new Texture2D(1, 1);
            _inputBackgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.2f, 0.95f));
            _inputBackgroundTexture.Apply();

            _texturesCreated = true;
        }

        public void Show()
        {
            _isVisible = true;
            _inputText = "";
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void AddOutput(string line)
        {
            _outputLines.Add(line);
            while (_outputLines.Count > MaxOutputLines)
            {
                _outputLines.RemoveAt(0);
            }
        }

        public void SetInputText(string text)
        {
            _inputText = text;
        }

        public void OnGUI()
        {
            if (!_isVisible) return;

            CreateTextures();

            GUI.depth = -10;

            _windowRect = GUI.Window(0, _windowRect, DrawWindow, "GM Console", GUI.skin.window);
        }

        private void DrawWindow(int windowID)
        {
            float yPos = 25;
            float contentHeight = _windowRect.height - 60;

            // Background
            GUI.DrawTexture(new Rect(0, yPos, _windowRect.width, contentHeight), _backgroundTexture);

            // Output area with scroll
            Rect outputRect = new Rect(5, yPos + 5, _windowRect.width - 10, contentHeight - 40);
            Rect scrollViewRect = new Rect(0, 0, outputRect.width - 20, _outputLines.Count * 18f);

            _scrollPosition = GUI.BeginScrollView(outputRect, _scrollPosition, scrollViewRect);

            float lineY = 0;
            GUIStyle lineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };

            foreach (string line in _outputLines)
            {
                Color textColor = Color.white;
                if (line.StartsWith(">"))
                    textColor = new Color(0.3f, 0.8f, 0.3f); // Green for commands
                else if (line.StartsWith("!"))
                    textColor = new Color(1f, 0.4f, 0.4f); // Red for errors
                else if (line.StartsWith("==="))
                    textColor = new Color(0.4f, 0.7f, 1f); // Blue for headers

                lineStyle.normal.textColor = textColor;
                GUI.Label(new Rect(5, lineY, scrollViewRect.width - 10, 18), line, lineStyle);
                lineY += 18;
            }

            GUI.EndScrollView();

            // Input field
            Rect inputRect = new Rect(5, _windowRect.height - 35, _windowRect.width - 10, 25);
            GUI.DrawTexture(inputRect, _inputBackgroundTexture);

            Rect labelRect = new Rect(10, _windowRect.height - 32, 20, 20);
            GUI.Label(labelRect, ">", new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.3f, 0.8f, 0.3f) }
            });

            Rect textFieldRect = new Rect(25, _windowRect.height - 32, inputRect.width - 35, 20);
            GUI.SetNextControlName("ConsoleInput");
            _inputText = GUI.TextField(textFieldRect, _inputText, new GUIStyle(GUI.skin.textField)
            {
                fontSize = 13,
                normal = { textColor = Color.white }
            });

            // Only focus input field on mouse click inside the window
            if (Event.current.type == EventType.MouseDown &&
                _windowRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
            {
                GUI.FocusControl("ConsoleInput");
            }

            // Handle Enter key for command execution
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (!string.IsNullOrWhiteSpace(_inputText))
                {
                    AddOutput($"> {_inputText}");
                    var result = CommandExecutor.Execute(_inputText);
                    if (result != null && !string.IsNullOrEmpty(result.Error))
                    {
                        AddOutput($"! Error: {result.Error}");
                    }
                    else if (result != null && result.Success)
                    {
                        AddOutput($"OK: {_inputText}");
                    }
                    _inputText = "";
                }
                Event.current.Use();
            }

            // Make window draggable
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, _windowRect.height));
        }
    }
}
