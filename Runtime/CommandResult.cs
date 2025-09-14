using System;

namespace Commander
{
    /// <summary>
    /// Representa o resultado de uma execução de comando
    /// </summary>
    public readonly struct CommandResult
    {
        /// <summary>
        /// Status da execução
        /// </summary>
        public CommandStatus Status { get; }

        /// <summary>
        /// Mensagem do resultado
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Exceção se ocorreu um erro
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Tempo de execução
        /// </summary>
        public TimeSpan ExecutionTime { get; }

        private CommandResult(CommandStatus status, string message, Exception exception = null,
            TimeSpan executionTime = default)
        {
            Status = status;
            Message = message ?? string.Empty;
            Exception = exception;
            ExecutionTime = executionTime;
        }

        /// <summary>
        /// Cria um resultado de sucesso
        /// </summary>
        public static CommandResult Success(string message = "", TimeSpan executionTime = default)
        {
            return new CommandResult(CommandStatus.Success, message, null, executionTime);
        }

        /// <summary>
        /// Cria um resultado de erro
        /// </summary>
        public static CommandResult Error(string message, Exception exception = null)
        {
            return new CommandResult(CommandStatus.Error, message, exception);
        }

        /// <summary>
        /// Cria um resultado de aviso
        /// </summary>
        public static CommandResult Warning(string message)
        {
            return new CommandResult(CommandStatus.Warning, message);
        }

        /// <summary>
        /// Cria um resultado informativo
        /// </summary>
        public static CommandResult Info(string message)
        {
            return new CommandResult(CommandStatus.Info, message);
        }
    }

    /// <summary>
    /// Status de execução do comando
    /// </summary>
    public enum CommandStatus
    {
        Success,
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// Interface para observadores de execução de comando
    /// </summary>
    public interface ICommandObserver
    {
        /// <summary>
        /// Chamado quando um comando é executado
        /// </summary>
        void OnCommandExecuted(CommandResult result);
    }
}