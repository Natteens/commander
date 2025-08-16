using System.Linq;
using UnityEngine;

namespace Commander
{
    public class AutoComplete
    {
        private readonly ICommandRegistry registry;
        
        public AutoComplete(ICommandRegistry registry)
        {
            this.registry = registry;
        }
        
        public string GetBestSuggestion(string input)
        {
            var suggestions = GetSuggestions(input, 1);
            return suggestions.FirstOrDefault() ?? string.Empty;
        }
        
        public string[] GetSuggestions(string input, int maxSuggestions = 5)
        {
            if (string.IsNullOrEmpty(input)) return System.Array.Empty<string>();
            
            var parts = input.Split(' ');
            
            if (parts.Length == 1)
            {
                return GetCommandSuggestions(parts[0], maxSuggestions);
            }
            else if (parts.Length == 2)
            {
                return GetTargetSuggestions(parts[0], parts[1], maxSuggestions);
            }
            
            return System.Array.Empty<string>();
        }
        
        private string[] GetCommandSuggestions(string partial, int maxSuggestions)
        {
            return registry.GetSuggestions(partial).Take(maxSuggestions).ToArray();
        }
        
        private string[] GetTargetSuggestions(string command, string partial, int maxSuggestions)
        {
            var cmd = registry.GetCommand(command);
            if (cmd == null) return System.Array.Empty<string>();
            
            var gameObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => IsValidTarget(go))
                .Where(go => go.name.ToLower().StartsWith(partial.ToLower()))
                .Select(go => $"{command} {go.name}")
                .Take(maxSuggestions)
                .ToArray();
                
            return gameObjects;
        }
        
        private bool IsValidTarget(GameObject go)
        {
            return go.activeInHierarchy && 
                   !go.name.StartsWith("UI") && 
                   !go.GetComponent<Camera>();
        }
    }
}