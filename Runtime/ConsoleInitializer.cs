using System;
using UnityEngine;

namespace Commander
{
    public class ConsoleInitializer : MonoBehaviour
    {
        private static ConsoleInitializer _instance;
        private static bool _initializationAttempted;
        private ConsoleUI consoleUI;
        private bool systemInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            // LIMPA estados anteriores ao iniciar
            CleanupStaleInstances();
            
            // APENAS inicializa em Development Builds ou no Editor
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                Debug.Log("Commander: Desabilitado em builds de produção");
                return;
            }

            if (_initializationAttempted)
            {
                return;
            }

            _initializationAttempted = true;

            // Se já existe uma instância, não cria outra
            if (_instance != null)
            {
                return;
            }

            CreateInstance();
        }

        private static void CleanupStaleInstances()
        {
            // Limpa qualquer instância antiga que possa ter sobrado
            _instance = null;
            _initializationAttempted = false;
            
            // Remove GameObjects órfãos do Commander
            var staleObjects = GameObject.FindObjectsOfType<ConsoleInitializer>();
            foreach (var obj in staleObjects)
            {
                if (obj != null && obj.gameObject != null)
                {
                    DestroyImmediate(obj.gameObject);
                }
            }
            
            var staleConsoles = GameObject.FindObjectsOfType<ConsoleUI>();
            foreach (var console in staleConsoles)
            {
                if (console != null && console.gameObject != null)
                {
                    DestroyImmediate(console.gameObject);
                }
            }
        }

        private static void CreateInstance()
        {
            // Verifica novamente se já existe
            if (_instance != null)
            {
                Debug.LogWarning("Commander: Tentativa de criar instância duplicada bloqueada");
                return;
            }

            var go = new GameObject("[Commander System]");
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            DontDestroyOnLoad(go);

            _instance = go.AddComponent<ConsoleInitializer>();
            _instance.InitializeSystem();
        }

        private void Awake()
        {
            // Proteção contra duplicatas
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Commander: Instância duplicada detectada e destruída");
                DestroyImmediate(gameObject);
                return;
            }

            // Verifica se está em build de produção
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                Debug.Log("Commander: Desabilitado em builds de produção");
                DestroyImmediate(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
#if UNITY_EDITOR
            // Limpa quando sair do Play Mode no Editor
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                // Força limpeza completa ao sair do Play Mode
                ForceCleanup();
            }
        }

        private void ForceCleanup()
        {
            try
            {
                if (consoleUI != null)
                {
                    consoleUI.Cleanup();
                    if (consoleUI.gameObject != null)
                        DestroyImmediate(consoleUI.gameObject);
                }

                _instance = null;
                _initializationAttempted = false;
                systemInitialized = false;

                if (gameObject != null)
                    DestroyImmediate(gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Commander: Erro na limpeza forçada: {ex.Message}");
            }
        }
#endif

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

            // Última verificação de segurança
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                Debug.Log("Commander: Sistema não inicializado (produção)");
                DestroyImmediate(gameObject);
                return;
            }

            try
            {
                CommandSystem.Initialize();
                consoleUI = ConsoleUI.Create();
                systemInitialized = true;
                Debug.Log("Commander: Sistema inicializado com sucesso");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Commander: Erro na inicialização: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

            if (consoleUI != null)
            {
                consoleUI.Cleanup();
            }

            if (_instance == this)
            {
                _instance = null;
                _initializationAttempted = false;
            }
        }

        public static bool IsActive()
        {
            return _instance != null && _instance.systemInitialized;
        }
    }
}