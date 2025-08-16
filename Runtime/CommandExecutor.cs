using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Commander
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ICommandRegistry registry;
        private readonly ParameterParser parameterParser;
        private readonly TargetResolver targetResolver;
        
        public event Action<CommandResult> OnCommandExecuted;
        
        public CommandExecutor(ICommandRegistry registry)
        {
            this.registry = registry;
            parameterParser = new ParameterParser();
            targetResolver = new TargetResolver();
        }
        
        public CommandResult Execute(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return CommandResult.Error("Comando vazio");
                
            var stopwatch = Stopwatch.StartNew();
            CommandResult result;
            
            try
            {
                var parts = ParseCommandLine(commandLine);
                var commandName = parts[0].ToLower();
                
                var command = registry.GetCommand(commandName);
                if (command == null)
                {
                    var suggestions = registry.GetSuggestions(commandName).Take(3);
                    var suggestionText = suggestions.Any() 
                        ? $" Você quis dizer: {string.Join(", ", suggestions)}?"
                        : "";
                    
                    result = CommandResult.Error($"Comando desconhecido: '{commandName}'.{suggestionText}");
                }
                else
                {
                    result = ExecuteCommand(command, parts.Skip(1).ToArray(), stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                result = CommandResult.Error($"Falha na execução do comando: {ex.Message}", ex);
            }
            finally
            {
                stopwatch.Stop();
            }
            
            OnCommandExecuted?.Invoke(result);
            return result;
        }
        
        private CommandResult ExecuteCommand(ICommand command, string[] args, TimeSpan executionTime)
        {
            try
            {
                var target = targetResolver.ResolveTarget(command, args);
                var skipArgs = target != null ? 1 : 0;
                var parameters = parameterParser.ParseParameters(command.ParameterTypes, args, skipArgs);
                
                if (!command.CanExecute(target))
                {
                    return CommandResult.Error($"Comando '{command.Name}' não pode ser executado com o alvo atual");
                }
                
                bool success = command.Execute(target, parameters);
                
                return success 
                    ? CommandResult.Success($"Comando '{command.Name}' executado com sucesso", null, executionTime)
                    : CommandResult.Error($"Falha na execução do comando '{command.Name}'");
            }
            catch (Exception ex)
            {
                return CommandResult.Error($"Erro no comando '{command.Name}': {ex.Message}", ex);
            }
        }
        
        private string[] ParseCommandLine(string commandLine)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            string current = "";
            
            for (int i = 0; i < commandLine.Length; i++)
            {
                char c = commandLine[i];
                
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
    }
}