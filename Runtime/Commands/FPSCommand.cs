using System;

namespace Commander.Commands
{
    public class FPSCommand : ICommand
    {
        private readonly OverlayRenderer overlayRenderer;
        private FPSOverlay fpsOverlay;
        
        public string Name => "fps";
        public string Description => "Toggle FPS display overlay";
        public string Category => "Debug";
        public Type[] ParameterTypes => new Type[] { typeof(bool) };
        
        public FPSCommand(OverlayRenderer overlayRenderer)
        {
            this.overlayRenderer = overlayRenderer;
        }
        
        public bool Execute(object target, params object[] parameters)
        {
            bool show = parameters.Length > 0 ? (bool)parameters[0] : ToggleFPS();
            
            if (show)
            {
                if (fpsOverlay == null)
                {
                    fpsOverlay = new FPSOverlay();
                    overlayRenderer.AddOverlay("fps", fpsOverlay);
                }
                fpsOverlay.IsVisible = true;
                ConsoleController.Log("FPS overlay enabled", CommandStatus.Success);
            }
            else
            {
                if (fpsOverlay != null)
                {
                    fpsOverlay.IsVisible = false;
                    overlayRenderer.RemoveOverlay("fps");
                }
                ConsoleController.Log("FPS overlay disabled", CommandStatus.Success);
            }
            
            return true;
        }
        
        public bool CanExecute(object target) => true;
        
        private bool ToggleFPS()
        {
            return fpsOverlay?.IsVisible != true;
        }
    }
    
    public class MemoryCommand : ICommand
    {
        private readonly OverlayRenderer overlayRenderer;
        private MemoryOverlay memoryOverlay;
        
        public string Name => "memory";
        public string Description => "Toggle memory display overlay";
        public string Category => "Debug";
        public Type[] ParameterTypes => new Type[] { typeof(bool) };
        
        public MemoryCommand(OverlayRenderer overlayRenderer)
        {
            this.overlayRenderer = overlayRenderer;
        }
        
        public bool Execute(object target, params object[] parameters)
        {
            if (parameters.Length == 0)
            {
                ShowMemoryInfo();
                return true;
            }
            
            bool show = (bool)parameters[0];
            
            if (show)
            {
                if (memoryOverlay == null)
                {
                    memoryOverlay = new MemoryOverlay();
                    overlayRenderer.AddOverlay("memory", memoryOverlay);
                }
                memoryOverlay.IsVisible = true;
                ConsoleController.Log("Memory overlay enabled", CommandStatus.Success);
            }
            else
            {
                if (memoryOverlay != null)
                {
                    memoryOverlay.IsVisible = false;
                    overlayRenderer.RemoveOverlay("memory");
                }
                ConsoleController.Log("Memory overlay disabled", CommandStatus.Success);
            }
            
            return true;
        }
        
        public bool CanExecute(object target) => true;
        
        private void ShowMemoryInfo()
        {
            var memory = GC.GetTotalMemory(false);
            ConsoleController.Log($"Memory usage: {memory / 1024 / 1024}MB", CommandStatus.Info);
            
            GC.Collect();
            var afterGC = GC.GetTotalMemory(true);
            ConsoleController.Log($"After GC: {afterGC / 1024 / 1024}MB", CommandStatus.Info);
        }
    }
}