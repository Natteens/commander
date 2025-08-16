using System;
using UnityEngine;

namespace Commander.Commands
{
    public class TimeScaleCommand : ICommand
    {
        public string Name => "time";
        public string Description => "Set time scale";
        public string Category => "Debug";
        public Type[] ParameterTypes => new Type[] { typeof(float) };
        
        public bool Execute(object target, params object[] parameters)
        {
            var scale = parameters.Length > 0 ? (float)parameters[0] : 1f;
            scale = Mathf.Clamp(scale, 0f, 10f);
            
            Time.timeScale = scale;
            
            var status = scale == 0f ? "paused" : 
                scale < 1f ? "slow motion" :
                scale > 1f ? "fast forward" : "normal";
            
            ConsoleController.Log($"Time scale set to {scale} ({status})", CommandStatus.Success);
            
            return true;
        }
        
        public bool CanExecute(object target) => true;
    }
}