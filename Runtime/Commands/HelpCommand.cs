using System;
using System.Linq;
using UnityEngine;

namespace Commander.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly ICommandRegistry registry;
        
        public string Name => "help";
        public string Description => "Show all available commands or help for specific command";
        public string Category => "System";
        public Type[] ParameterTypes => new Type[] { typeof(string) };
        
        public HelpCommand(ICommandRegistry registry)
        {
            this.registry = registry;
        }
        
        public bool Execute(object target, params object[] parameters)
        {
            var commandName = parameters.Length > 0 ? parameters[0]?.ToString() : "";
            
            if (!string.IsNullOrEmpty(commandName))
            {
                var command = registry.GetCommand(commandName);
                if (command != null)
                {
                    ShowCommandHelp(command);
                    return true;
                }
                else
                {
                    ConsoleController.Log($"Command '{commandName}' not found", CommandStatus.Error);
                    return false;
                }
            }
            
            ConsoleController.Log("=== COMMANDER CONSOLE HELP ===", CommandStatus.Info);
            ConsoleController.Log("Use TAB for autocomplete, arrows for history", CommandStatus.Info);
            ConsoleController.Log("", CommandStatus.Info);
            
            ConsoleController.Log("SYNTAX EXAMPLES:", CommandStatus.Info);
            ConsoleController.Log("• Vectors: teleport (2,3,4) or teleport 2 3 4", CommandStatus.Info);
            ConsoleController.Log("• Floats: Use . or , as decimal separator", CommandStatus.Info);
            ConsoleController.Log("• Booleans: true/false, 1/0, on/off", CommandStatus.Info);
            ConsoleController.Log("• Colors: red, #FF0000, (255,0,0)", CommandStatus.Info);
            ConsoleController.Log("• Strings with spaces: \"hello world\"", CommandStatus.Info);
            ConsoleController.Log("", CommandStatus.Info);
            
            var commands = registry.GetAllCommands()
                .GroupBy(cmd => cmd.Category)
                .OrderBy(g => g.Key);
            
            foreach (var group in commands)
            {
                ConsoleController.Log($"[{group.Key}]", CommandStatus.Info);
                foreach (var cmd in group.OrderBy(c => c.Name))
                {
                    var usage = GetCommandUsage(cmd);
                    ConsoleController.Log($"  {usage} - {cmd.Description}", CommandStatus.Info);
                }
                ConsoleController.Log("", CommandStatus.Info);
            }
            
            return true;
        }
        
        private void ShowCommandHelp(ICommand command)
        {
            ConsoleController.Log($"=== HELP: {command.Name.ToUpper()} ===", CommandStatus.Info);
            ConsoleController.Log($"Description: {command.Description}", CommandStatus.Info);
            ConsoleController.Log($"Category: {command.Category}", CommandStatus.Info);
            ConsoleController.Log($"Usage: {GetCommandUsage(command)}", CommandStatus.Info);
            
            if (command.ParameterTypes.Length > 0)
            {
                ConsoleController.Log("Parameters:", CommandStatus.Info);
                for (int i = 0; i < command.ParameterTypes.Length; i++)
                {
                    var paramType = command.ParameterTypes[i];
                    var typeName = GetFriendlyTypeName(paramType);
                    ConsoleController.Log($"  {i + 1}. {typeName}", CommandStatus.Info);
                }
            }
        }
        
        private string GetCommandUsage(ICommand cmd)
        {
            var usage = cmd.Name;
            
            foreach (var paramType in cmd.ParameterTypes)
            {
                var typeName = GetFriendlyTypeName(paramType);
                usage += $" <{typeName}>";
            }
            
            return usage;
        }
        
        private string GetFriendlyTypeName(Type type)
        {
            return type.Name switch
            {
                "String" => "text",
                "Int32" => "number",
                "Single" => "decimal",
                "Boolean" => "true/false",
                "Vector3" => "x,y,z",
                "Vector2" => "x,y",
                "Color" => "color",
                _ => type.Name.ToLower()
            };
        }
        
        public bool CanExecute(object target) => true;
    }
}