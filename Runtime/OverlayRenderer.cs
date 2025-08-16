using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commander
{
    public class OverlayRenderer
    {
        private Dictionary<string, IOverlay> overlays = new Dictionary<string, IOverlay>();
        private GUIStyle overlayStyle;
        private bool stylesInitialized;
        
        public void AddOverlay(string id, IOverlay overlay)
        {
            overlays[id] = overlay;
        }
        
        public void RemoveOverlay(string id)
        {
            overlays.Remove(id);
        }
        
        public void UpdateOverlays()
        {
            foreach (var overlay in overlays.Values)
            {
                overlay.Update();
            }
        }
        
        public void OnGUI()
        {
            if (!stylesInitialized)
                InitializeStyles();
                
            foreach (var overlay in overlays.Values)
            {
                if (overlay.IsVisible)
                    overlay.OnGUI(overlayStyle);
            }
        }
        
        private void InitializeStyles()
        {
            overlayStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { 
                    background = CreateTexture(new Color(0, 0, 0, 0.8f)),
                    textColor = Color.white
                },
                fontSize = 12,
                padding = new RectOffset(8, 8, 4, 4),
                alignment = TextAnchor.MiddleLeft
            };
            
            stylesInitialized = true;
        }
        
        private Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
    
    public interface IOverlay
    {
        bool IsVisible { get; }
        void Update();
        void OnGUI(GUIStyle style);
    }
    
    public class FPSOverlay : IOverlay
    {
        private float fps;
        private float frameTime;
        private float updateInterval = 0.5f;
        private float lastUpdate;
        
        public bool IsVisible { get; set; } = true;
        
        public void Update()
        {
            if (Time.unscaledTime - lastUpdate >= updateInterval)
            {
                fps = 1f / Time.unscaledDeltaTime;
                frameTime = Time.unscaledDeltaTime * 1000f;
                lastUpdate = Time.unscaledTime;
            }
        }
        
        public void OnGUI(GUIStyle style)
        {
            var content = $"FPS: {fps:F1}\nFrame: {frameTime:F1}ms";
            var size = style.CalcSize(new GUIContent(content));
            var rect = new Rect(10, 10, size.x, size.y);
            
            GUI.Box(rect, content, style);
        }
    }
    
    public class MemoryOverlay : IOverlay
    {
        private long memoryUsage;
        private float updateInterval = 2f;
        private float lastUpdate;
        
        public bool IsVisible { get; set; } = true;
        
        public void Update()
        {
            if (Time.unscaledTime - lastUpdate >= updateInterval)
            {
                memoryUsage = GC.GetTotalMemory(false);
                lastUpdate = Time.unscaledTime;
            }
        }
        
        public void OnGUI(GUIStyle style)
        {
            var content = $"Memory: {memoryUsage / 1024 / 1024}MB";
            var size = style.CalcSize(new GUIContent(content));
            var rect = new Rect(10, 60, size.x, size.y);
            
            GUI.Box(rect, content, style);
        }
    }
}