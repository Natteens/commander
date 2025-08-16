using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Commander
{
    public class ConsoleUI
    {
        private readonly ICommandExecutor executor;
        private readonly ICommandRegistry registry;
        
        private bool isVisible;
        private string inputText = "";
        private List<LogEntry> logEntries = new List<LogEntry>();
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private int scrollOffset = 0;
        
        private GUIStyle logStyle;
        private GUIStyle inputStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized;
        
        private AutoComplete autoComplete;
        private bool showDeveloperAuth;
        private string developerInput = "";
        private Action<bool> developerCallback;
        private string expectedKey;
        
        private const int LINE_HEIGHT = 16;
        private const int PADDING = 8;
        private const int INPUT_HEIGHT = 20;
        private const int MAX_LOG_ENTRIES = 200;
        
        public ConsoleUI(ConsoleUIConfig config, ICommandExecutor executor, ICommandRegistry registry)
        {
            this.executor = executor;
            this.registry = registry;
            
            autoComplete = new AutoComplete(registry);
        }
        
        public void ClearLog()
        {
            logEntries.Clear();
            scrollOffset = 0;
        }
        
        public void Toggle()
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                inputText = "";
                historyIndex = -1;
            }
        }
        
        public void OnGUI()
        {
            if (!stylesInitialized)
                InitializeStyles();
                
            if (showDeveloperAuth)
            {
                DrawDeveloperAuth();
                return;
            }
            
            if (!isVisible) return;
            
            DrawCMDStyleConsole();
        }
        
        private void DrawCMDStyleConsole()
        {
            float consoleWidth = 800f;
            float consoleHeight = 400f;
            
            Rect consoleRect = new Rect(
                10, 
                Screen.height - consoleHeight - 10, 
                consoleWidth, 
                consoleHeight
            );
            
            GUI.color = Color.black;
            GUI.DrawTexture(consoleRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            Rect borderRect = new Rect(consoleRect.x - 1, consoleRect.y - 1, consoleRect.width + 2, consoleRect.height + 2);
            GUI.color = Color.white;
            GUI.DrawTexture(borderRect, Texture2D.whiteTexture);
            GUI.color = Color.black;
            GUI.DrawTexture(consoleRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            Rect innerRect = new Rect(
                consoleRect.x + PADDING, 
                consoleRect.y + PADDING, 
                consoleRect.width - (PADDING * 2), 
                consoleRect.height - (PADDING * 2)
            );
            
            Rect logRect = new Rect(
                innerRect.x, 
                innerRect.y, 
                innerRect.width, 
                innerRect.height - INPUT_HEIGHT - PADDING
            );
            
            DrawLogArea(logRect);
            
            Rect inputRect = new Rect(
                innerRect.x, 
                innerRect.yMax - INPUT_HEIGHT, 
                innerRect.width, 
                INPUT_HEIGHT
            );
            
            DrawInputArea(inputRect);
            
            HandleScrollInput();
            
            if (!string.IsNullOrEmpty(inputText))
            {
                DrawSuggestions(inputRect);
            }
        }
        
        private void DrawLogArea(Rect logRect)
        {
            int visibleLines = Mathf.FloorToInt(logRect.height / LINE_HEIGHT);
            int totalLines = logEntries.Count;
            
            int maxScroll = Mathf.Max(0, totalLines - visibleLines);
            scrollOffset = Mathf.Clamp(scrollOffset, 0, maxScroll);
            
            for (int i = 0; i < visibleLines && i < totalLines; i++)
            {
                int entryIndex = scrollOffset + i;
                if (entryIndex >= totalLines) break;
                
                var entry = logEntries[entryIndex];
                
                Rect lineRect = new Rect(
                    logRect.x, 
                    logRect.y + (i * LINE_HEIGHT), 
                    logRect.width, 
                    LINE_HEIGHT
                );
                
                Color originalColor = GUI.contentColor;
                GUI.contentColor = GetStatusColor(entry.Status);
                
                string content = $"[{entry.Timestamp:HH:mm:ss}] {entry.Message}";
                GUI.Label(lineRect, content, logStyle);
                
                GUI.contentColor = originalColor;
            }
            
            if (totalLines > visibleLines)
            {
                DrawScrollbar(logRect, totalLines, visibleLines);
            }
            
            if (scrollOffset == maxScroll - 1 || scrollOffset == maxScroll)
            {
                scrollOffset = maxScroll;
            }
        }
        
        private void DrawScrollbar(Rect logRect, int totalLines, int visibleLines)
        {
            float scrollbarWidth = 16f;
            Rect scrollbarRect = new Rect(
                logRect.xMax - scrollbarWidth, 
                logRect.y, 
                scrollbarWidth, 
                logRect.height
            );
            
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            GUI.DrawTexture(scrollbarRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            float thumbHeight = (float)visibleLines / totalLines * scrollbarRect.height;
            float thumbY = (float)scrollOffset / (totalLines - visibleLines) * (scrollbarRect.height - thumbHeight);
            
            Rect thumbRect = new Rect(
                scrollbarRect.x + 2, 
                scrollbarRect.y + thumbY, 
                scrollbarRect.width - 4, 
                thumbHeight
            );
            
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            GUI.DrawTexture(thumbRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            if (Event.current.type == EventType.MouseDown && scrollbarRect.Contains(Event.current.mousePosition))
            {
                float clickRatio = (Event.current.mousePosition.y - scrollbarRect.y) / scrollbarRect.height;
                scrollOffset = Mathf.RoundToInt(clickRatio * (totalLines - visibleLines));
                Event.current.Use();
            }
        }
        
        private void HandleScrollInput()
        {
            if (!isVisible) return;
            
            Event e = Event.current;
            
            if (e.type == EventType.ScrollWheel)
            {
                scrollOffset += Mathf.RoundToInt(e.delta.y * 3);
                int maxScroll = Mathf.Max(0, logEntries.Count - Mathf.FloorToInt((Screen.height * 0.4f) / LINE_HEIGHT));
                scrollOffset = Mathf.Clamp(scrollOffset, 0, maxScroll);
                e.Use();
            }
            
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.PageUp)
                {
                    scrollOffset -= 10;
                    scrollOffset = Mathf.Max(0, scrollOffset);
                    e.Use();
                }
                else if (e.keyCode == KeyCode.PageDown)
                {
                    scrollOffset += 10;
                    int maxScroll = Mathf.Max(0, logEntries.Count - Mathf.FloorToInt((Screen.height * 0.4f) / LINE_HEIGHT));
                    scrollOffset = Mathf.Min(scrollOffset, maxScroll);
                    e.Use();
                }
            }
        }
        
        private void DrawInputArea(Rect inputRect)
        {
            GUI.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            GUI.DrawTexture(inputRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            Rect promptRect = new Rect(inputRect.x + 2, inputRect.y + 2, 12, inputRect.height - 4);
            GUI.Label(promptRect, ">", logStyle);
            
            Rect textRect = new Rect(
                inputRect.x + 16, 
                inputRect.y + 2, 
                inputRect.width - 18, 
                inputRect.height - 4
            );
            
            GUI.SetNextControlName("ConsoleInput");
            
            Event currentEvent = Event.current;
            
            if (currentEvent.type == EventType.KeyDown)
            {
                HandleKeyboardInput(currentEvent);
            }
            
            string newInput = GUI.TextField(textRect, inputText, inputStyle);
            if (newInput != inputText && currentEvent.type != EventType.Layout)
            {
                inputText = newInput;
            }
            
            if (isVisible)
            {
                GUI.FocusControl("ConsoleInput");
            }
        }
        
        private void HandleKeyboardInput(Event e)
        {
            if (GUI.GetNameOfFocusedControl() != "ConsoleInput") return;
            
            switch (e.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        ExecuteCommand(inputText.Trim());
                        commandHistory.Add(inputText.Trim());
                        if (commandHistory.Count > 50)
                            commandHistory.RemoveAt(0);
                    }
                    inputText = "";
                    historyIndex = -1;
                    
                    int maxScroll = Mathf.Max(0, logEntries.Count - Mathf.FloorToInt((Screen.height * 0.4f) / LINE_HEIGHT));
                    scrollOffset = maxScroll;
                    
                    e.Use();
                    break;
                    
                case KeyCode.UpArrow:
                    if (commandHistory.Count > 0)
                    {
                        historyIndex = Mathf.Min(historyIndex + 1, commandHistory.Count - 1);
                        inputText = commandHistory[commandHistory.Count - 1 - historyIndex];
                    }
                    e.Use();
                    break;
                    
                case KeyCode.DownArrow:
                    if (historyIndex > 0)
                    {
                        historyIndex--;
                        inputText = commandHistory[commandHistory.Count - 1 - historyIndex];
                    }
                    else if (historyIndex == 0)
                    {
                        historyIndex = -1;
                        inputText = "";
                    }
                    e.Use();
                    break;
                    
                case KeyCode.Tab:
                    var suggestion = autoComplete.GetBestSuggestion(inputText);
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        inputText = suggestion;
                    }
                    e.Use();
                    break;
            }
        }
        
        private void DrawSuggestions(Rect inputRect)
        {
            var suggestions = autoComplete.GetSuggestions(inputText, 5);
            if (!suggestions.Any()) return;
            
            float suggestionHeight = 16f;
            float totalHeight = suggestions.Length * suggestionHeight;
            
            Rect suggestionRect = new Rect(
                inputRect.x + 16, 
                inputRect.y - totalHeight - 2,
                300,
                totalHeight
            );
            
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            GUI.DrawTexture(suggestionRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            Rect borderRect = new Rect(suggestionRect.x - 1, suggestionRect.y - 1, suggestionRect.width + 2, suggestionRect.height + 2);
            GUI.color = Color.gray;
            GUI.DrawTexture(borderRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            GUI.DrawTexture(suggestionRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            for (int i = 0; i < suggestions.Length; i++)
            {
                Rect itemRect = new Rect(
                    suggestionRect.x + 2,
                    suggestionRect.y + i * suggestionHeight,
                    suggestionRect.width - 4,
                    suggestionHeight
                );
                
                if (GUI.Button(itemRect, suggestions[i], buttonStyle))
                {
                    inputText = suggestions[i];
                    Event.current.Use();
                }
            }
        }
        
        private void DrawDeveloperAuth()
        {
            var authRect = new Rect(
                Screen.width * 0.5f - 200, 
                Screen.height * 0.5f - 60, 
                400, 
                120
            );
            
            GUI.color = Color.black;
            GUI.DrawTexture(authRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            GUILayout.BeginArea(new Rect(authRect.x + 10, authRect.y + 10, authRect.width - 20, authRect.height - 20));
            
            GUILayout.Label("Enter developer key:", logStyle);
            GUI.SetNextControlName("DeveloperInput");
            developerInput = GUILayout.TextField(developerInput, inputStyle);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Confirm"))
            {
                bool success = developerInput == expectedKey;
                developerCallback?.Invoke(success);
                showDeveloperAuth = false;
                developerInput = "";
            }
            
            if (GUILayout.Button("Cancel"))
            {
                showDeveloperAuth = false;
                developerInput = "";
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            GUI.FocusControl("DeveloperInput");
        }
        
        private void InitializeStyles()
        {
            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = Color.white },
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            
            inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 11,
                normal = { 
                    textColor = Color.white,
                    background = null
                },
                focused = { 
                    textColor = Color.white,
                    background = null
                },
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0)
            };
            
            buttonStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = Color.white },
                hover = { textColor = Color.cyan },
                padding = new RectOffset(4, 4, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };
            
            stylesInitialized = true;
        }
        
        private Color GetStatusColor(CommandStatus status)
        {
            return status switch
            {
                CommandStatus.Success => Color.green,
                CommandStatus.Error => Color.red,
                CommandStatus.Warning => Color.yellow,
                _ => Color.white
            };
        }
        
        private void ExecuteCommand(string commandLine)
        {
            AddLogEntry($"> {commandLine}", CommandStatus.Info);
            executor.Execute(commandLine);
        }
        
        public void AddLogEntry(string message, CommandStatus status)
        {
            logEntries.Add(new LogEntry(message, status, DateTime.Now));
            
            if (logEntries.Count > MAX_LOG_ENTRIES)
                logEntries.RemoveAt(0);
        }
        
        public void ShowDeveloperAuth(string expectedKey, Action<bool> callback)
        {
            this.expectedKey = expectedKey;
            this.developerCallback = callback;
            showDeveloperAuth = true;
        }
        
        private struct LogEntry
        {
            public string Message { get; }
            public CommandStatus Status { get; }
            public DateTime Timestamp { get; }
            
            public LogEntry(string message, CommandStatus status, DateTime timestamp)
            {
                Message = message;
                Status = status;
                Timestamp = timestamp;
            }
        }
    }
}