using System;
using System.Linq;
using UnityEngine;

namespace Commander
{
    public class TargetResolver
    {
        public object ResolveTarget(ICommand command, string[] args)
        {
            if (args.Length == 0) return null;
            
            var targetName = args[0];
            var targetObject = GameObject.Find(targetName);
            
            if (targetObject == null)
            {
                var allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                targetObject = allObjects.FirstOrDefault(go => 
                    go.name.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            
            return targetObject;
        }
    }
}