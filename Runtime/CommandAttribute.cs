using System;

namespace Commander
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string Category { get; set; } = "Game";
        
        public CommandAttribute(string name, string description = "")
        {
            Name = name.ToLower();
            Description = description;
        }
    }
}