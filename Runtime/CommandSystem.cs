using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Commander
{
    public static class CommandSystem
    {
        private static readonly Dictionary<string, CommandMethod> Commands = new();
        private static readonly List<ICommandObserver> Observers = new();
        private static readonly ParameterConverter Converter = new();
        private static bool _isInitialized;
        
        public static void Initialize()
        {
            if (_isInitialized) return;

            ScanForCommands();
            _isInitialized = true;
            
            Log("Sistema Commander inicializado", CommandStatus.Success);
        }

        public static CommandResult Execute(string input)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrWhiteSpace(input))
                return CommandResult.Error("Comando vazio");

            var parts = ParseInput(input);
            var commandName = parts[0].ToLower();

            if (!Commands.TryGetValue(commandName, out var command))
                return CommandResult.Error($"Comando '{commandName}' não encontrado");

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var success = ExecuteCommandMethod(command, parts.Skip(1).ToArray());
                stopwatch.Stop();

                var result = success 
                    ? CommandResult.Success($"Comando '{commandName}' executado", stopwatch.Elapsed)
                    : CommandResult.Error($"Falha ao executar '{commandName}'");
                    
                NotifyObservers(result);
                return result;
            }
            catch (Exception ex)
            {
                var errorResult = CommandResult.Error($"Erro ao executar '{commandName}': {ex.Message}", ex);
                NotifyObservers(errorResult);
                return errorResult;
            }
        }

        public static string[] GetCommands()
        {
            if (!_isInitialized) Initialize();
            return Commands.Keys.OrderBy(x => x).ToArray();
        }
        
        public static string[] GetSuggestions(string partial)
        {
            if (!_isInitialized) Initialize();
            if (string.IsNullOrEmpty(partial)) return Array.Empty<string>();

            return Commands.Keys
                .Where(cmd => cmd.StartsWith(partial.ToLower()))
                .OrderBy(cmd => cmd)
                .ToArray();
        }
        
        public static void AddObserver(ICommandObserver observer)
        {
            if (observer != null && !Observers.Contains(observer))
                Observers.Add(observer);
        }

        
        public static void RemoveObserver(ICommandObserver observer)
        {
            Observers.Remove(observer);
        }

        
        public static void Log(string message, CommandStatus status = CommandStatus.Info)
        {
            ConsoleUI.Instance?.AddLog(message, status);
        }

        private static void ScanForCommands()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies.Where(ShouldScanAssembly))
                ScanAssembly(assembly);
        }

        private static bool ShouldScanAssembly(Assembly assembly)
        {
            var name = assembly.FullName;
            return !name.Contains("Unity.") &&
                   !name.Contains("System.") &&
                   !name.Contains("Microsoft.") &&
                   !name.Contains("Mono.") &&
                   !name.Contains("ImGui") &&   
                   !name.Contains("ImPlot") &&   
                   !name.Contains("netstandard") &&
                   !name.Contains("mscorlib");
        }

        private static void ScanAssembly(Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                    ScanType(type);
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var type in ex.Types.Where(t => t != null))
                {
                    try
                    {
                        ScanType(type);
                    }
                    catch (Exception typeEx)
                    {
                        if (!IsImGuiRelatedType(type))
                        {
                            UnityEngine.Debug.LogWarning($"Commander: Erro ao escanear tipo individual {type?.Name}: {typeEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Commander: Erro ao escanear assembly {assembly.FullName}: {ex.Message}");
            }
        }

        private static void ScanType(Type type)
        {
            try
            {
                if (IsImGuiRelatedType(type))
                    return;

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    try
                    {
                        var attribute = method.GetCustomAttribute<CommandAttribute>();
                        if (attribute != null)
                        {
                            var commandMethod = new CommandMethod(attribute.Name, method, attribute.Description);
                            Commands[attribute.Name] = commandMethod;
                        }
                    }
                    catch (Exception methodEx)
                    {
                        if (!type.FullName.Contains("ImGui") && !type.FullName.Contains("ImPlot"))
                        {
                            UnityEngine.Debug.LogWarning($"Commander: Erro ao processar método {method.Name} em {type.Name}: {methodEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsImGuiRelatedType(type))
                {
                    UnityEngine.Debug.LogWarning($"Commander: Erro ao escanear tipo {type?.Name}: {ex.Message}");
                }
            }
        }

        private static bool IsImGuiRelatedType(Type type)
        {
            if (type == null) return false;
            
            var fullName = type.FullName;
            if (string.IsNullOrEmpty(fullName)) return false;

            return fullName.Contains("ImGui") || 
                   fullName.Contains("ImPlot") || 
                   fullName.Contains("cimgui") ||
                   type.Namespace?.Contains("ImGui") == true ||
                   type.Namespace?.Contains("ImPlot") == true;
        }

        private static string[] ParseInput(string input)
        {
            var parts = new List<string>();
            var current = "";
            var inQuotes = false;

            foreach (var c in input)
            {
                if (c == '"' || c == '\'')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrEmpty(current))
                parts.Add(current);

            return parts.ToArray();
        }

        private static bool ExecuteCommandMethod(CommandMethod command, string[] args)
        {
            try
            {
                var parameters = command.Method.GetParameters();
                var convertedArgs = new object[parameters.Length];

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (i < args.Length)
                        convertedArgs[i] = Converter.Convert(args[i], parameters[i].ParameterType);
                    else if (parameters[i].HasDefaultValue)
                        convertedArgs[i] = parameters[i].DefaultValue;
                    else
                        convertedArgs[i] = GetDefaultValue(parameters[i].ParameterType);
                }

                var target = command.Method.IsStatic ? null : GetInstance(command.Method.DeclaringType);
                command.Method.Invoke(target, convertedArgs);
                return true;
            }
            catch (Exception ex)
            {
                Log($"Erro na execução do comando: {ex.Message}", CommandStatus.Error);
                return false;
            }
        }

        private static object GetInstance(Type type)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                var instance = UnityEngine.Object.FindFirstObjectByType(type);
                if (instance != null) return instance;
                var go = new GameObject($"Commander_{type.Name}");
                UnityEngine.Object.DontDestroyOnLoad(go);
                return go.AddComponent(type);
            }

            return Activator.CreateInstance(type);
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static void NotifyObservers(CommandResult result)
        {
            foreach (var observer in Observers.ToList())
            {
                try
                {
                    observer.OnCommandExecuted(result);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Commander: Erro no observer: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Representa um método de comando
    /// </summary>
    internal sealed class CommandMethod
    {
        public CommandMethod(string name, MethodInfo method, string description = "")
        {
            Name = name;
            Method = method;
            Description = description;
        }

        public string Name { get; }
        public MethodInfo Method { get; }
        public string Description { get; }
    }
}