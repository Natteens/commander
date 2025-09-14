using System;

namespace Commander
{
    /// <summary>
    /// Marca um método como um comando de console
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Inicializa um novo atributo de comando
        /// </summary>
        /// <param name="name">O nome do comando</param>
        /// <param name="description">Descrição do comando (opcional)</param>
        public CommandAttribute(string name, string description = "")
        {
            Name = name?.ToLower() ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }

        /// <summary>
        /// Nome do comando
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Descrição do comando
        /// </summary>
        public string Description { get; }
    }
}