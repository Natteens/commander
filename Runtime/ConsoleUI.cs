using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Commander
{
    /// <summary>
    /// Console funcional com autocomplete melhorado
    /// </summary>
    public sealed class ConsoleUI : MonoBehaviour, ICommandObserver
    {
        private const int MaxLogEntries = 1000;
        private const int MaxHistoryEntries = 50;
        private const int MaxSuggestions = 8;

        private static ConsoleUI _instance;
        public static ConsoleUI Instance => _instance;

        private readonly List<string> commandHistory = new();
        private readonly List<LogEntry> logs = new();
        
        private string inputBuffer = "";
        private int historyIndex = -1;
        private bool isVisible = false;
        private bool shouldFocusInput = false;
        
        // Window
        private Rect windowRect = new Rect(20, 0, 600, 400);
        
        // Scroll
        private Vector2 scrollPosition = Vector2.zero;
        private bool userIsScrolling = false;
        private float lastScrollTime = 0f;
        
        // Autocomplete - Melhorado
        private string[] currentSuggestions = new string[0];
        private int selectedSuggestion = -1;
        private bool showSuggestions = false;
        private string lastInputForSuggestions = "";
        private Rect suggestionRect;
        private Vector2 suggestionScrollPosition = Vector2.zero;
        
        private readonly Dictionary<CommandStatus, Color> logColors = new()
        {
            { CommandStatus.Success, Color.green },
            { CommandStatus.Error, Color.red },
            { CommandStatus.Warning, Color.yellow },
            { CommandStatus.Info, Color.cyan }
        };

        private float fps = 0f;
        private int frameCount = 0;
        private float lastFpsUpdate = 0f;

        // Estilos GUI (cached)
        private GUIStyle suggestionBoxStyle;
        private GUIStyle suggestionItemStyle;
        private GUIStyle suggestionSelectedStyle;
        private GUIStyle suggestionHeaderStyle;
        private bool stylesInitialized = false;

        public static ConsoleUI Create()
        {
            if (_instance != null) 
            {
                DestroyImmediate(_instance.gameObject);
            }

            var go = new GameObject("[Commander Console]");
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            var console = go.AddComponent<ConsoleUI>();
            DontDestroyOnLoad(go);
            return console;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            // Estilo da caixa de sugestões
            suggestionBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(4, 4, 4, 4),
                normal = { background = MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f, 0.95f)) }
            };

            // Estilo do item de sugestão
            suggestionItemStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(2, 2, 1, 1),
                normal = { 
                    textColor = Color.white,
                    background = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f, 0.8f))
                },
                hover = {
                    textColor = Color.white,
                    background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f, 0.9f))
                }
            };

            // Estilo do item selecionado
            suggestionSelectedStyle = new GUIStyle(suggestionItemStyle)
            {
                normal = {
                    textColor = Color.black,
                    background = MakeTex(1, 1, new Color(0.7f, 0.9f, 1f, 0.9f))
                },
                hover = {
                    textColor = Color.black,
                    background = MakeTex(1, 1, new Color(0.8f, 0.95f, 1f, 0.95f))
                }
            };

            // Estilo do cabeçalho das sugestões
            suggestionHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 6, 2, 2),
                normal = { textColor = new Color(1f, 1f, 0.5f, 1f) }
            };

            stylesInitialized = true;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            _instance = this;
            CommandSystem.AddObserver(this);
            
            // Posição inicial
            windowRect = new Rect(20, Screen.height - 420, 600, 400);
            
            AddLog("=== Commander Console ===", CommandStatus.Info);
            AddLog("F1, F12 ou ` para abrir/fechar", CommandStatus.Info);
            AddLog("Tab para mostrar sugestões", CommandStatus.Info);
            
            Debug.Log("Commander: Console inicializado");
        }

        private void OnDisable()
        {
            isVisible = false;
        }

        public void OnCommandExecuted(CommandResult result)
        {
            if (!string.IsNullOrEmpty(result.Message) || result.Status == CommandStatus.Error)
            {
                var message = string.IsNullOrEmpty(result.Message) ? 
                    $"OK ({result.ExecutionTime.TotalMilliseconds:F1}ms)" : 
                    result.Message;
                AddLog(message, result.Status);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                isVisible = false;
                return;
            }

            UpdateSystemInfo();
            HandleInputKeys();
            UpdateScrollBehavior();
        }

        private void UpdateSystemInfo()
        {
            frameCount++;
            if (Time.unscaledTime - lastFpsUpdate >= 1f)
            {
                fps = frameCount / (Time.unscaledTime - lastFpsUpdate);
                frameCount = 0;
                lastFpsUpdate = Time.unscaledTime;
            }
        }

        private void HandleInputKeys()
        {
            // Toggle console
            if (Input.GetKeyDown(KeyCode.F1) || 
                Input.GetKeyDown(KeyCode.F12) || 
                Input.GetKeyDown(KeyCode.BackQuote) ||
                Input.GetKeyDown(KeyCode.F2))
            {
                Toggle();
            }

            if (!isVisible) return;

            // Histórico
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }

            // ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (showSuggestions)
                {
                    HideSuggestions();
                }
                else
                {
                    Hide();
                }
            }
        }

        private void UpdateScrollBehavior()
        {
            if (Time.unscaledTime - lastScrollTime > 3f)
            {
                userIsScrolling = false;
            }
        }

        private void OnGUI()
        {
            if (!isVisible || !Application.isPlaying) return;

            InitializeStyles();

            // Posição fixa no canto inferior esquerdo
            windowRect.x = 20;
            windowRect.y = Screen.height - windowRect.height - 20;
            windowRect.width = Mathf.Clamp(windowRect.width, 400, Screen.width - 40);
            windowRect.height = Mathf.Clamp(windowRect.height, 250, Screen.height - 40);

            // Fundo da janela
            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            GUI.Box(windowRect, "");
            GUI.color = Color.white;

            // Conteúdo principal
            GUILayout.BeginArea(windowRect);
            GUILayout.BeginVertical();

            DrawHeader();
            DrawLogsArea();
            DrawInputArea();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Desenha sugestões FORA da área principal para evitar clipping
            if (showSuggestions && currentSuggestions.Length > 0)
            {
                DrawSuggestionsOverlay();
            }

            // Permite arrastar
            if (Event.current.type == EventType.MouseDrag && windowRect.Contains(Event.current.mousePosition))
            {
                windowRect.x += Event.current.delta.x;
                windowRect.y += Event.current.delta.y;
            }
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            GUILayout.Label($"Commander | FPS: {fps:F0} | Logs: {logs.Count}", 
                new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", GUILayout.Width(40), GUILayout.Height(18)))
            {
                Clear();
            }

            if (GUILayout.Button("×", GUILayout.Width(18), GUILayout.Height(18)))
            {
                Hide();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawLogsArea()
        {
            var logHeight = windowRect.height - 80;

            var newScrollPosition = GUILayout.BeginScrollView(
                scrollPosition, 
                GUILayout.Height(logHeight)
            );

            if (Vector2.Distance(newScrollPosition, scrollPosition) > 2f)
            {
                userIsScrolling = true;
                lastScrollTime = Time.unscaledTime;
            }
            scrollPosition = newScrollPosition;

            foreach (var log in logs)
            {
                var color = logColors[log.Status];
                GUI.color = color;

                var prefix = GetLogPrefix(log.Status);
                var timestamp = $"[{log.Timestamp:HH:mm:ss}] ";
                
                GUILayout.Label($"{timestamp}{prefix}{log.Message}", 
                    new GUIStyle(GUI.skin.label) { 
                        fontSize = 11, 
                        wordWrap = true,
                        normal = { textColor = color }
                    });
            }
            
            GUI.color = Color.white;

            if (!userIsScrolling && Event.current.type == EventType.Repaint)
            {
                scrollPosition.y = float.MaxValue;
            }

            GUILayout.EndScrollView();
        }

        private void DrawInputArea()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(">", new GUIStyle(GUI.skin.label) { 
                fontSize = 12, 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.green }
            }, GUILayout.Width(12));

            GUI.SetNextControlName("CommandInput");
            
            // Eventos do input
            Event e = Event.current;
            if (e.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == "CommandInput")
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        if (showSuggestions && selectedSuggestion >= 0 && selectedSuggestion < currentSuggestions.Length)
                        {
                            ApplySuggestion(currentSuggestions[selectedSuggestion]);
                        }
                        else
                        {
                            ExecuteCommand();
                        }
                        e.Use();
                        break;

                    case KeyCode.Tab:
                        HandleTabCompletion();
                        e.Use();
                        break;

                    case KeyCode.UpArrow when showSuggestions:
                        selectedSuggestion = selectedSuggestion <= 0 ? currentSuggestions.Length - 1 : selectedSuggestion - 1;
                        e.Use();
                        break;

                    case KeyCode.DownArrow when showSuggestions:
                        selectedSuggestion = (selectedSuggestion + 1) % currentSuggestions.Length;
                        e.Use();
                        break;
                }
            }

            var newInput = GUILayout.TextField(inputBuffer, 
                new GUIStyle(GUI.skin.textField) { 
                    fontSize = 12,
                    normal = { textColor = Color.white },
                    focused = { textColor = Color.white }
                });
            
            if (newInput != inputBuffer)
            {
                inputBuffer = newInput;
                OnInputChanged();
            }

            if (shouldFocusInput)
            {
                GUI.FocusControl("CommandInput");
                shouldFocusInput = false;
            }

            GUILayout.EndHorizontal();

            // Calcula a posição das sugestões baseado no input
            if (Event.current.type == EventType.Repaint)
            {
                var inputRect = GUILayoutUtility.GetLastRect();
                suggestionRect = new Rect(
                    windowRect.x + 20, // Margem da janela
                    windowRect.y + windowRect.height - 60, // Acima do input
                    windowRect.width - 40, // Largura da janela menos margens
                    0 // Altura será calculada dinamicamente
                );
            }
        }

        private void DrawSuggestionsOverlay()
        {
            var maxHeight = 200f;
            var itemHeight = 22f;
            var headerHeight = 20f;
            var totalItems = Mathf.Min(currentSuggestions.Length, MaxSuggestions);
            var contentHeight = headerHeight + (totalItems * itemHeight);
            var finalHeight = Mathf.Min(contentHeight, maxHeight);

            suggestionRect.height = finalHeight;
            suggestionRect.y = windowRect.y + windowRect.height - 60 - finalHeight; // Posiciona acima do input

            // Garante que não saia da tela
            if (suggestionRect.y < 10)
            {
                suggestionRect.y = windowRect.y + windowRect.height + 10; // Coloca abaixo se não couber acima
            }

            // Fundo da caixa de sugestões
            GUI.Box(suggestionRect, "", suggestionBoxStyle);

            GUILayout.BeginArea(suggestionRect);
            GUILayout.BeginVertical();

            // Cabeçalho
            GUILayout.Label($"Sugestões ({currentSuggestions.Length} encontradas)", suggestionHeaderStyle);

            // Lista de sugestões com scroll se necessário
            if (contentHeight > maxHeight)
            {
                suggestionScrollPosition = GUILayout.BeginScrollView(
                    suggestionScrollPosition,
                    GUILayout.Height(maxHeight - headerHeight)
                );
            }

            for (int i = 0; i < totalItems; i++)
            {
                var suggestion = currentSuggestions[i];
                var isSelected = i == selectedSuggestion;
                var style = isSelected ? suggestionSelectedStyle : suggestionItemStyle;
                
                var displayText = $"{i + 1}. {suggestion}";
                if (isSelected)
                {
                    displayText = "▶ " + displayText;
                }

                if (GUILayout.Button(displayText, style, GUILayout.Height(itemHeight)))
                {
                    ApplySuggestion(suggestion);
                }

                // Destaca a seleção atual
                if (isSelected && Event.current.type == EventType.Repaint)
                {
                    var buttonRect = GUILayoutUtility.GetLastRect();
                    GUI.color = new Color(1f, 1f, 0.5f, 0.3f);
                    GUI.DrawTexture(buttonRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }

            if (contentHeight > maxHeight)
            {
                GUILayout.EndScrollView();
            }

            // Rodapé com dicas
            GUILayout.FlexibleSpace();
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            GUILayout.Label("↑↓: navegar | Enter/Click: aplicar | Tab: próxima | ESC: fechar", 
                new GUIStyle(GUI.skin.label) { 
                    fontSize = 9, 
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.7f, 0.7f, 0.7f, 1f) }
                });
            GUI.color = Color.white;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void OnInputChanged()
        {
            // Auto-atualiza sugestões enquanto digita (opcional)
            if (showSuggestions)
            {
                UpdateSuggestions();
                selectedSuggestion = 0;
            }
        }

        private void HandleTabCompletion()
        {
            if (!showSuggestions)
            {
                // Primeira vez pressionando Tab - mostra sugestões
                UpdateSuggestions();
                if (currentSuggestions.Length > 0)
                {
                    showSuggestions = true;
                    selectedSuggestion = 0;
                }
            }
            else
            {
                // Tab subsequentes - navega pelas sugestões
                selectedSuggestion = (selectedSuggestion + 1) % currentSuggestions.Length;
            }
        }

        private void UpdateSuggestions()
        {
            if (string.IsNullOrEmpty(inputBuffer))
            {
                currentSuggestions = new string[0];
                return;
            }

            // Evita recalcular se o input não mudou
            if (inputBuffer == lastInputForSuggestions && showSuggestions)
                return;

            var suggestions = CommandSystem.GetSuggestions(inputBuffer).Take(MaxSuggestions).ToArray();
            currentSuggestions = suggestions;
            lastInputForSuggestions = inputBuffer;
            
            if (currentSuggestions.Length == 0)
            {
                HideSuggestions();
            }
        }

        private void ApplySuggestion(string suggestion)
        {
            inputBuffer = suggestion + " "; // Adiciona espaço para facilitar digitação de parâmetros
            HideSuggestions();
            shouldFocusInput = true;
        }

        private void HideSuggestions()
        {
            showSuggestions = false;
            selectedSuggestion = -1;
            lastInputForSuggestions = "";
            suggestionScrollPosition = Vector2.zero;
        }

        private void NavigateHistory(int direction)
        {
            if (commandHistory.Count == 0) return;

            historyIndex = Mathf.Clamp(historyIndex + direction, -1, commandHistory.Count - 1);
            
            if (historyIndex >= 0)
            {
                inputBuffer = commandHistory[commandHistory.Count - 1 - historyIndex];
            }
            else
            {
                inputBuffer = "";
            }
            
            HideSuggestions();
            shouldFocusInput = true;
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrWhiteSpace(inputBuffer)) return;

            var command = inputBuffer.Trim();
            AddLog($"> {command}", CommandStatus.Info);

            if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != command)
            {
                commandHistory.Add(command);
                if (commandHistory.Count > MaxHistoryEntries)
                    commandHistory.RemoveAt(0);
            }

            try
            {
                CommandSystem.Execute(command);
            }
            catch (Exception ex)
            {
                AddLog($"ERRO: {ex.Message}", CommandStatus.Error);
            }

            inputBuffer = "";
            historyIndex = -1;
            HideSuggestions();
            shouldFocusInput = true;
            userIsScrolling = false;
        }

        private static string GetLogPrefix(CommandStatus status)
        {
            return status switch
            {
                CommandStatus.Success => "[✓] ",
                CommandStatus.Error => "[✗] ",
                CommandStatus.Warning => "[!] ",
                CommandStatus.Info => "[i] ",
                _ => "[?] "
            };
        }

        public void Toggle()
        {
            if (!Application.isPlaying) return;
            
            isVisible = !isVisible;
            
            if (isVisible)
            {
                inputBuffer = "";
                historyIndex = -1;
                HideSuggestions();
                shouldFocusInput = true;
                userIsScrolling = false;
            }
        }

        public void Show()
        {
            if (!Application.isPlaying) return;
            isVisible = true;
            shouldFocusInput = true;
        }

        public void Hide()
        {
            isVisible = false;
            HideSuggestions();
        }

        public void Clear()
        {
            logs.Clear();
            AddLog("Console limpo", CommandStatus.Info);
            userIsScrolling = false;
        }

        public void AddLog(string message, CommandStatus status)
        {
            logs.Add(new LogEntry(message, status, DateTime.Now));
            
            if (logs.Count > MaxLogEntries)
                logs.RemoveAt(0);
        }

        public void Cleanup()
        {
            try
            {
                CommandSystem.RemoveObserver(this);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Commander: Erro cleanup: {ex.Message}");
            }
            
            if (_instance == this)
                _instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private readonly struct LogEntry : IEquatable<LogEntry>
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

            public bool Equals(LogEntry other)
            {
                return Message == other.Message && 
                       Status == other.Status && 
                       Timestamp.Equals(other.Timestamp);
            }

            public override bool Equals(object obj)
            {
                return obj is LogEntry other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Message, Status, Timestamp);
            }

            public static bool operator ==(LogEntry left, LogEntry right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(LogEntry left, LogEntry right)
            {
                return !left.Equals(right);
            }
        }
    }
}