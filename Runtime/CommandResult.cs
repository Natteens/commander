using System;

namespace Commander
{
    public enum CommandStatus
    {
        Success,
        Error,
        Warning,
        Info
    }
    
    public struct CommandResult
    {
        public CommandStatus Status { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public object ReturnValue { get; }
        public TimeSpan ExecutionTime { get; }
        
        public CommandResult(CommandStatus status, string message, object returnValue = null, Exception exception = null, TimeSpan executionTime = default)
        {
            Status = status;
            Message = message;
            ReturnValue = returnValue;
            Exception = exception;
            ExecutionTime = executionTime;
        }
        
        public static CommandResult Success(string message, object returnValue = null, TimeSpan executionTime = default) =>
            new CommandResult(CommandStatus.Success, message, returnValue, null, executionTime);
            
        public static CommandResult Error(string message, Exception exception = null) =>
            new CommandResult(CommandStatus.Error, message, null, exception);
            
        public static CommandResult Warning(string message) =>
            new CommandResult(CommandStatus.Warning, message);
            
        public static CommandResult Info(string message) =>
            new CommandResult(CommandStatus.Info, message);
    }
}