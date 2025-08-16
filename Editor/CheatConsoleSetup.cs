#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Commander.Editor
{
    public class CheatConsoleSetup : EditorWindow
    {
        [MenuItem("Tools/Console/Setup")]
        public static void ShowWindow()
        {
            GetWindow<CheatConsoleSetup>("Console Setup");
        }
        
        void OnGUI()
        {
            EditorGUILayout.LabelField("Development Console Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawConsoleStatus();
            DrawUsageInfo();
        }
        
        private void DrawConsoleStatus()
        {
            var console = Object.FindFirstObjectByType<ConsoleController>();
            
            if (console == null)
            {
                EditorGUILayout.HelpBox("Console not found in scene.", MessageType.Warning);
                
                if (GUILayout.Button("Create Console"))
                {
                    CreateConsole();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Console configured in scene.", MessageType.Info);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Select Console"))
                {
                    Selection.activeGameObject = console.gameObject;
                }
                
                if (GUILayout.Button("Test Console"))
                {
                    console.GetConsoleUI()?.Toggle();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
        }
        
        private void DrawUsageInfo()
        {
            EditorGUILayout.LabelField("Usage Instructions", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Runtime Controls:");
            EditorGUILayout.LabelField("• Press F1 to toggle console");
            EditorGUILayout.LabelField("• Type 'help' for command list");
            EditorGUILayout.LabelField("• Use TAB for autocomplete");
            EditorGUILayout.LabelField("• Use arrows for history navigation");
            EditorGUILayout.LabelField("• Use mouse wheel or Page Up/Down to scroll");
            EditorGUILayout.LabelField("• Ctrl+Shift+F12 for dev auth in builds");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Example Commands:");
            EditorGUILayout.LabelField("• list - List available objects");
            EditorGUILayout.LabelField("• fps - Toggle FPS overlay");
            EditorGUILayout.LabelField("• time 0.5 - Set slow motion");
            EditorGUILayout.LabelField("• memory - Show memory usage");
            EditorGUILayout.LabelField("• clear - Clear console log");
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enable 'Show Debug Logs' in the Console Controller to see system messages in Unity Console.", MessageType.Info);
        }
        
        private void CreateConsole()
        {
            var consoleGO = new GameObject("ConsoleController");
            var console = consoleGO.AddComponent<ConsoleController>();
            
            Selection.activeGameObject = consoleGO;
            EditorGUIUtility.PingObject(consoleGO);
        }
    }
}
#endif