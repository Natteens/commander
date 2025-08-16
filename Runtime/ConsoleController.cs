using System;
using UnityEngine;
using Commander.Commands;

namespace Commander
{
    public class ConsoleController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private string developerKey = "DEV_2024";
        [SerializeField] private bool enableInEditor = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = false;
        
        [Header("UI Configuration")]
        [SerializeField] private ConsoleUIConfig uiConfig = ConsoleUIConfig.Default;
        
        private ICommandRegistry commandRegistry;
        private ICommandExecutor commandExecutor;
        private ConsoleUI consoleUI;
        private OverlayRenderer overlayRenderer;
        private bool isEnabled;
        
        public static ConsoleController Instance { get; private set; }
        public static event Action<CommandResult> OnCommandExecuted;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeConsole();
        }
        
        private void Start()
        {
            #if UNITY_EDITOR
            isEnabled = enableInEditor;
            #else
            isEnabled = true;
            #endif
            
            if (!isEnabled)
                gameObject.SetActive(false);
        }
        
        private void Update()
        {
            HandleInput();
            overlayRenderer?.UpdateOverlays();
        }
        
        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == toggleKey && isEnabled)
            {
                consoleUI.Toggle();
                Event.current.Use();
                return;
            }
            
            consoleUI?.OnGUI();
            overlayRenderer?.OnGUI();
        }
        
        private void InitializeConsole()
        {
            commandRegistry = new CommandRegistry();
            commandExecutor = new CommandExecutor(commandRegistry);
            overlayRenderer = new OverlayRenderer();
            consoleUI = new ConsoleUI(uiConfig, commandExecutor, commandRegistry);
            
            RegisterBuiltInCommands();
            ScanForCommands();
            
            commandExecutor.OnCommandExecuted += HandleCommandResult;
            
            LogDebug("Console initialized. Type 'help' for available commands.", CommandStatus.Info);
        }
        
        private void HandleInput()
        {
            if (!isEnabled && Input.GetKey(KeyCode.LeftControl) && 
                Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F12))
            {
                consoleUI.ShowDeveloperAuth(developerKey, OnDeveloperAuth);
            }
        }
        
        private void OnDeveloperAuth(bool success)
        {
            if (success)
            {
                isEnabled = true;
                Log("Developer mode activated", CommandStatus.Success);
            }
            else
            {
                Log("Invalid developer key", CommandStatus.Error);
            }
        }
        
        private void RegisterBuiltInCommands()
        {
            commandRegistry.Register(new HelpCommand(commandRegistry));
            commandRegistry.Register(new ListCommand());
            commandRegistry.Register(new FindCommand());
            commandRegistry.Register(new ClearCommand(consoleUI));
            commandRegistry.Register(new TimeScaleCommand());
            commandRegistry.Register(new FPSCommand(overlayRenderer));
            commandRegistry.Register(new MemoryCommand(overlayRenderer));
            commandRegistry.Register(new QuitCommand());
        }
        
        private void ScanForCommands()
        {
            var scanner = new CommandScanner();
            var discoveredCommands = scanner.ScanAssemblies();
            
            foreach (var command in discoveredCommands)
            {
                commandRegistry.Register(command);
            }
            
            LogDebug($"Registered {commandRegistry.GetAllCommands().Length} commands", CommandStatus.Info);
        }
        
        private void HandleCommandResult(CommandResult result)
        {
            Log(result.Message, result.Status);
            OnCommandExecuted?.Invoke(result);
        }
        
        public static void Log(string message, CommandStatus status = CommandStatus.Info)
        {
            if (Instance?.consoleUI != null)
            {
                Instance.consoleUI.AddLogEntry(message, status);
            }
            
            if (Instance?.showDebugLogs == true)
            {
                switch (status)
                {
                    case CommandStatus.Error:
                        Debug.LogError($"[Console] {message}");
                        break;
                    case CommandStatus.Warning:
                        Debug.LogWarning($"[Console] {message}");
                        break;
                    default:
                        Debug.Log($"[Console] {message}");
                        break;
                }
            }
        }
        
        public static void LogDebug(string message, CommandStatus status = CommandStatus.Info)
        {
            if (Instance?.consoleUI != null)
            {
                Instance.consoleUI.AddLogEntry(message, status);
            }
            
            if (Instance?.showDebugLogs == true)
            {
                switch (status)
                {
                    case CommandStatus.Error:
                        Debug.LogError($"[Console Debug] {message}");
                        break;
                    case CommandStatus.Warning:
                        Debug.LogWarning($"[Console Debug] {message}");
                        break;
                    default:
                        Debug.Log($"[Console Debug] {message}");
                        break;
                }
            }
        }
        
        public static void ExecuteCommand(string commandLine)
        {
            Instance?.commandExecutor.Execute(commandLine);
        }
        
        public ICommandRegistry GetCommandRegistry()
        {
            return commandRegistry;
        }
        
        public ConsoleUI GetConsoleUI()
        {
            return consoleUI;
        }
        
        public void SetUIConfig(ConsoleUIConfig newConfig)
        {
            uiConfig = newConfig;
            if (consoleUI != null)
            {
                consoleUI = new ConsoleUI(uiConfig, commandExecutor, commandRegistry);
            }
        }
    }
}