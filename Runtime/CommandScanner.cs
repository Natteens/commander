using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Commander
{
    public class CommandScanner
    {
        public IEnumerable<ICommand> ScanAssemblies()
        {
            var commands = new List<ICommand>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                if (ShouldSkipAssembly(assembly)) continue;
                
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        commands.AddRange(ScanType(type));
                    }
                }
                catch (Exception ex)
                {
                    ConsoleController.LogDebug($"Failed to scan assembly {assembly.GetName().Name}: {ex.Message}", CommandStatus.Warning);
                }
            }
            
            return commands;
        }
        
        private bool ShouldSkipAssembly(Assembly assembly)
        {
            var name = assembly.FullName;
            return name.Contains("JetBrains") || 
                   name.Contains("Rider") || 
                   name.Contains("Unity.") ||
                   name.Contains("System.");
        }
        
        private IEnumerable<ICommand> ScanType(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                        BindingFlags.Instance | BindingFlags.Static);
            
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();
                if (attr != null)
                {
                    yield return new ReflectionCommand(attr, method);
                }
            }
        }
    }
    
    public class ReflectionCommand : ICommand
    {
        private readonly CommandAttribute attribute;
        private readonly MethodInfo method;
        
        public string Name => attribute.Name;
        public string Description => attribute.Description;
        public string Category => attribute.Category;
        public Type[] ParameterTypes => method.GetParameters().Select(p => p.ParameterType).ToArray();
        
        public ReflectionCommand(CommandAttribute attribute, MethodInfo method)
        {
            this.attribute = attribute;
            this.method = method;
        }
        
        public bool Execute(object target, params object[] parameters)
        {
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, parameters);
                }
                else
                {
                    if (target == null)
                    {
                        var component = UnityEngine.Object.FindFirstObjectByType(method.DeclaringType);
                        if (component == null)
                        {
                            ConsoleController.Log($"No instance of {method.DeclaringType.Name} found", CommandStatus.Error);
                            return false;
                        }
                        target = component;
                    }
                    
                    method.Invoke(target, parameters);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                ConsoleController.Log($"Command execution failed: {ex.Message}", CommandStatus.Error);
                return false;
            }
        }
        
        public bool CanExecute(object target)
        {
            if (method.IsStatic) return true;
            return target != null || UnityEngine.Object.FindFirstObjectByType(method.DeclaringType) != null;
        }
    }
}