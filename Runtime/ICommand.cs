using System;

namespace Commander
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        string Category { get; }
        Type[] ParameterTypes { get; }
        bool Execute(object target, params object[] parameters);
        bool CanExecute(object target);
    }
    
    public interface ICommandRegistry
    {
        void Register(ICommand command);
        void Unregister(string commandName);
        ICommand GetCommand(string commandName);
        ICommand[] GetAllCommands();
        string[] GetSuggestions(string partial);
    }
    
    public interface ICommandExecutor
    {
        CommandResult Execute(string commandLine);
        event Action<CommandResult> OnCommandExecuted;
    }
}