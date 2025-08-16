using System;
using System.Linq;
using UnityEngine;

namespace Commander.Commands
{
    public class ListCommand : ICommand
    {
        public string Name => "list";
        public string Description => "List available game objects";
        public string Category => "Debug";
        public Type[] ParameterTypes => new Type[] { typeof(string) };
        
        public bool Execute(object target, params object[] parameters)
        {
            var filter = parameters.Length > 0 ? parameters[0]?.ToString() : "";
            
            var objects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => IsValidTarget(go))
                .Where(go => string.IsNullOrEmpty(filter) || go.name.ToLower().Contains(filter.ToLower()))
                .Take(20);
            
            ConsoleController.Log("=== AVAILABLE OBJECTS ===", CommandStatus.Info);
            foreach (var obj in objects)
            {
                var components = obj.GetComponents<Component>()
                    .Where(c => c != null && !(c is Transform))
                    .Select(c => c.GetType().Name)
                    .Take(3);
                
                var componentList = components.Any() ? $" ({string.Join(", ", components)})" : "";
                ConsoleController.Log($"  {obj.name}{componentList}", CommandStatus.Info);
            }
            
            return true;
        }
        
        public bool CanExecute(object target) => true;
        
        private bool IsValidTarget(GameObject go)
        {
            return go.activeInHierarchy && 
                   !go.name.StartsWith("UI") && 
                   !go.GetComponent<Camera>() &&
                   !go.GetComponent<ConsoleController>();
        }
    }
}