using System;
using UnityEngine;

namespace Commander
{
    public class ConsoleInitializer : MonoBehaviour
    {
        private static ConsoleInitializer _instance;
        private ConsoleUI consoleUI;
        private bool systemInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (_instance != null)
            {
                if (_instance.gameObject != null)
                    DestroyImmediate(_instance.gameObject);
            }

            CreateInstance();
        }

        private static void CreateInstance()
        {
            var go = new GameObject("[Commander System]");
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            DontDestroyOnLoad(go);

            _instance = go.AddComponent<ConsoleInitializer>();
            _instance.InitializeSystem();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!systemInitialized)
            {
                InitializeSystem();
            }
        }

        private void InitializeSystem()
        {
            if (systemInitialized) return;

            try
            {
                CommandSystem.Initialize();
                consoleUI = ConsoleUI.Create();
                systemInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Commander: Erro na inicialização: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            if (consoleUI != null)
            {
                consoleUI.Cleanup();
            }

            if (_instance == this)
                _instance = null;
        }

    }
}