#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Commander.Editor
{
    [CustomEditor(typeof(ConsoleController))]
    public class ConsoleControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Basic Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("developerKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableInEditor"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            
            var showDebugProperty = serializedObject.FindProperty("showDebugLogs");
            EditorGUILayout.PropertyField(showDebugProperty, new GUIContent("Show Debug Logs", "Enable debug logs in Unity Console"));
            
            if (showDebugProperty.boolValue)
            {
                EditorGUILayout.HelpBox("Debug logs are enabled. You'll see console system messages in Unity Console.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Debug logs are disabled. Only command results will appear in Unity Console.", MessageType.None);
            }
            
            EditorGUILayout.Space();
            
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
                
                var console = target as ConsoleController;
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Toggle Console"))
                {
                    console?.GetConsoleUI()?.Toggle();
                }
                
                if (GUILayout.Button("Clear Log"))
                {
                    console?.GetConsoleUI()?.ClearLog();
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("Execute Test Command"))
                {
                    ConsoleController.ExecuteCommand("help");
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Commands:", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("list", GUILayout.Height(20)))
                {
                    ConsoleController.ExecuteCommand("list");
                }
                if (GUILayout.Button("fps", GUILayout.Height(20)))
                {
                    ConsoleController.ExecuteCommand("fps");
                }
                if (GUILayout.Button("memory", GUILayout.Height(20)))
                {
                    ConsoleController.ExecuteCommand("memory");
                }
                EditorGUILayout.EndHorizontal();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif