using System;
using System.Collections.Generic;
using System.Linq;

namespace Commander
{
    public class CommandRegistry : ICommandRegistry
    {
        private Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
        
        public void Register(ICommand command)
        {
            if (command == null) 
            {
                ConsoleController.LogDebug("Cannot register null command", CommandStatus.Error);
                return;
            }
            
            if (commands.ContainsKey(command.Name))
            {
                ConsoleController.LogDebug($"Command '{command.Name}' already registered. Overwriting.", CommandStatus.Warning);
            }
            
            commands[command.Name] = command;
        }
        
        public void Unregister(string commandName)
        {
            commands.Remove(commandName?.ToLower());
        }
        
        public ICommand GetCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName)) return null;
            
            commands.TryGetValue(commandName.ToLower(), out var command);
            return command;
        }
        
        public ICommand[] GetAllCommands()
        {
            return commands.Values.ToArray();
        }
        
        public string[] GetSuggestions(string partial)
        {
            if (string.IsNullOrEmpty(partial)) return Array.Empty<string>();
            
            return commands.Keys
                .Where(cmd => cmd.StartsWith(partial.ToLower()))
                .OrderBy(cmd => cmd)
                .ToArray();
        }
    }
}