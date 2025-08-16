using System;

namespace Commander.Commands
{
    public class QuitCommand : ICommand
    {
        public string Name => "quit";
        public string Description => "Quit application";
        public string Category => "System";
        public Type[] ParameterTypes => Array.Empty<Type>();
        
        public bool Execute(object target, params object[] parameters)
        {
            ConsoleController.Log("Quitting application...", CommandStatus.Info);
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            
            return true;
        }
        
        public bool CanExecute(object target) => true;
    }
}