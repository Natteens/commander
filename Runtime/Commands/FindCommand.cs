using System;

namespace Commander.Commands
{
    public class FindCommand : ICommand
    {
        public string Name => "find";
        public string Description => "Find objects by name";
        public string Category => "Debug";
        public Type[] ParameterTypes => new Type[] { typeof(string) };
        
        public bool Execute(object target, params object[] parameters)
        {
            if (parameters.Length == 0)
            {
                ConsoleController.Log("Usage: find <search_term>", CommandStatus.Error);
                return false;
            }
            
            var listCommand = new ListCommand();
            return listCommand.Execute(target, parameters);
        }
        
        public bool CanExecute(object target) => true;
    }
}