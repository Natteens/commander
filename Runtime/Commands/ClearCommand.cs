using System;

namespace Commander.Commands
{
    public class ClearCommand : ICommand
    {
        private readonly ConsoleUI consoleUI;
        
        public string Name => "clear";
        public string Description => "Clear console log";
        public string Category => "System";
        public Type[] ParameterTypes => Array.Empty<Type>();
        
        public ClearCommand(ConsoleUI consoleUI)
        {
            this.consoleUI = consoleUI;
        }
        
        public bool Execute(object target, params object[] parameters)
        {
            consoleUI?.ClearLog();
            ConsoleController.Log("Console cleared", CommandStatus.Success);
            return true;
        }
        
        public bool CanExecute(object target) => true;
    }
}