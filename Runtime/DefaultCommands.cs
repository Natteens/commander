using System.Linq;
using UnityEngine;

namespace Commander
{
    /// <summary>
    /// Comandos padrão aprimorados para Dear ImGui
    /// </summary>
    internal static class DefaultCommands
    {
        [Command("help", "Mostra todos os comandos disponíveis com suas descrições")]
        public static void ShowHelp()
        {
            var commands = CommandSystem.GetCommands();
            CommandSystem.Log($"=== Commander Console - {commands.Length} comandos disponíveis ===", CommandStatus.Info);
            
            // Agrupa comandos por categoria (baseado no primeiro nome)
            var grouped = commands.GroupBy(cmd => cmd.Split('_')[0]).OrderBy(g => g.Key);
            
            foreach (var group in grouped)
            {
                CommandSystem.Log($"\n[{group.Key.ToUpper()}]", CommandStatus.Info);
                foreach (var cmd in group.OrderBy(x => x))
                {
                    CommandSystem.Log($"  {cmd}", CommandStatus.Success);
                }
            }
            
            CommandSystem.Log("\nDigite o nome do comando para executá-lo.", CommandStatus.Info);
            CommandSystem.Log("Use Tab para autocompletar e setas para navegar no histórico.", CommandStatus.Info);
        }

        [Command("clear", "Limpa o console")]
        public static void ClearConsole()
        {
            ConsoleUI.Instance?.Clear();
        }

        [Command("echo", "Repete a mensagem fornecida")]
        public static void Echo(string message)
        {
            CommandSystem.Log($"Echo: {message}", CommandStatus.Info);
        }

        [Command("system_info", "Mostra informações do sistema")]
        public static void ShowSystemInfo()
        {
            CommandSystem.Log("=== Informações do Sistema ===", CommandStatus.Info);
            CommandSystem.Log($"Unity Version: {Application.unityVersion}", CommandStatus.Info);
            CommandSystem.Log($"Platform: {Application.platform}", CommandStatus.Info);
            CommandSystem.Log($"Product Name: {Application.productName}", CommandStatus.Info);
            CommandSystem.Log($"FPS Target: {Application.targetFrameRate}", CommandStatus.Info);
            CommandSystem.Log($"VSync: {QualitySettings.vSyncCount}", CommandStatus.Info);
            CommandSystem.Log($"Time Scale: {Time.timeScale}", CommandStatus.Info);
            CommandSystem.Log($"Physics Gravity: {Physics.gravity.y}", CommandStatus.Info);
        }

        [Command("time_scale", "Define a escala de tempo do jogo")]
        public static void SetTimeScale(float timeScale = 1.0f)
        {
            Time.timeScale = Mathf.Max(0f, timeScale);
            CommandSystem.Log($"Escala de tempo: {Time.timeScale:F2}", CommandStatus.Success);
        }

        [Command("pause", "Pausa/despausa o jogo")]
        public static void TogglePause()
        {
            Time.timeScale = Time.timeScale > 0 ? 0 : 1;
            CommandSystem.Log($"Jogo {(Time.timeScale > 0 ? "despausado" : "pausado")}", CommandStatus.Success);
        }

        [Command("fps_target", "Define o FPS alvo da aplicação")]
        public static void SetTargetFPS(int fps = 60)
        {
            Application.targetFrameRate = fps > 0 ? fps : -1;
            var target = Application.targetFrameRate == -1 ? "Ilimitado" : Application.targetFrameRate.ToString();
            CommandSystem.Log($"FPS alvo: {target}", CommandStatus.Success);
        }

        [Command("vsync", "Liga/desliga o VSync (0=off, 1=on, 2=half)")]
        public static void SetVSync(int mode = 1)
        {
            QualitySettings.vSyncCount = Mathf.Clamp(mode, 0, 2);
            var modeText = QualitySettings.vSyncCount switch
            {
                0 => "Desligado",
                1 => "Ligado",
                2 => "Half Rate",
                _ => "Desconhecido"
            };
            CommandSystem.Log($"VSync: {modeText}", CommandStatus.Success);
        }

        [Command("gravity", "Define a gravidade do Physics")]
        public static void SetGravity(float gravity = 9.81f)
        {
            Physics.gravity = new Vector3(0, -Mathf.Abs(gravity), 0);
            CommandSystem.Log($"Gravidade: {gravity:F2}", CommandStatus.Success);
        }

        [Command("quit", "Sai da aplicação")]
        public static void QuitApplication()
        {
            CommandSystem.Log("Saindo da aplicação em 2 segundos...", CommandStatus.Warning);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        [Command("spawn_cube", "Cria um cubo na posição especificada")]
        public static void SpawnCube(float x = 0, float y = 0, float z = 0)
        {
            var position = new Vector3(x, y, z);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.name = $"CommanderCube_{Random.Range(1000, 9999)}";
            CommandSystem.Log($"Cubo criado em: ({x:F1}, {y:F1}, {z:F1})", CommandStatus.Success);
        }

        [Command("spawn_sphere", "Cria uma esfera na posição especificada")]
        public static void SpawnSphere(float x = 0, float y = 0, float z = 0)
        {
            var position = new Vector3(x, y, z);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.name = $"CommanderSphere_{Random.Range(1000, 9999)}";
            CommandSystem.Log($"Esfera criada em: ({x:F1}, {y:F1}, {z:F1})", CommandStatus.Success);
        }

        [Command("find_objects", "Encontra objetos por nome")]
        public static void FindObjects(string namePattern)
        {
            var objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var matches = objects.Where(obj => obj.name.ToLower().Contains(namePattern.ToLower()))
                                 .Select(obj => obj.name)
                                 .ToList();

            if (matches.Count > 0)
            {
                CommandSystem.Log($"Objetos encontrados ({matches.Count}):", CommandStatus.Info);
                foreach (var match in matches.Take(10)) // Limita a 10 para não poluir
                {
                    CommandSystem.Log($"  - {match}", CommandStatus.Success);
                }
                if (matches.Count > 10)
                    CommandSystem.Log($"  ... e mais {matches.Count - 10} objetos", CommandStatus.Info);
            }
            else
            {
                CommandSystem.Log($"Nenhum objeto encontrado com: '{namePattern}'", CommandStatus.Warning);
            }
        }

        [Command("destroy_object", "Destrói um objeto por nome")]
        public static void DestroyObject(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
                CommandSystem.Log($"Objeto destruído: {objectName}", CommandStatus.Success);
            }
            else
            {
                CommandSystem.Log($"Objeto não encontrado: {objectName}", CommandStatus.Warning);
            }
        }

        [Command("scene_objects", "Lista todos os objetos da cena")]
        public static void ListSceneObjects()
        {
            var objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
            
            CommandSystem.Log($"=== Objetos da Cena ({objects.Length}) ===", CommandStatus.Info);
            
            var rootObjects = objects.Where(obj => obj.transform.parent == null).ToList();
            foreach (var obj in rootObjects.Take(20)) // Limita para não poluir
            {
                CommandSystem.Log($"- {obj.name} ({obj.transform.childCount} filhos)", CommandStatus.Info);
            }
            
            if (rootObjects.Count > 20)
                CommandSystem.Log($"... e mais {rootObjects.Count - 20} objetos raiz", CommandStatus.Warning);
        }

        [Command("random", "Gera um número aleatório entre min e max")]
        public static void RandomNumber(int min = 0, int max = 100)
        {
            var result = Random.Range(min, max + 1);
            CommandSystem.Log($"Número aleatório [{min}-{max}]: {result}", CommandStatus.Info);
        }

        [Command("scene_info", "Mostra informações da cena atual")]
        public static void ShowSceneInfo()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            CommandSystem.Log("=== Informações da Cena ===", CommandStatus.Info);
            CommandSystem.Log($"Nome: {scene.name}", CommandStatus.Info);
            CommandSystem.Log($"Path: {scene.path}", CommandStatus.Info);
            CommandSystem.Log($"GameObjects: {gameObjects.Length}", CommandStatus.Info);
            CommandSystem.Log($"Carregada: {scene.isLoaded}", CommandStatus.Info);
        }

        [Command("memory_info", "Mostra informações de memória")]
        public static void ShowMemoryInfo()
        {
            // Usando as APIs atualizadas que não são obsoletas
            var totalMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            var reservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            var unusedMemory = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong();

            CommandSystem.Log("=== Informações de Memória ===", CommandStatus.Info);
            CommandSystem.Log($"Total Alocada: {totalMemory / 1024 / 1024:F1} MB", CommandStatus.Info);
            CommandSystem.Log($"Total Reservada: {reservedMemory / 1024 / 1024:F1} MB", CommandStatus.Info);
            CommandSystem.Log($"Não Utilizada: {unusedMemory / 1024 / 1024:F1} MB", CommandStatus.Info);
        }

        [Command("performance_info", "Mostra informações de performance")]
        public static void ShowPerformanceInfo()
        {
            CommandSystem.Log("=== Informações de Performance ===", CommandStatus.Info);
            CommandSystem.Log($"FPS Atual: {1.0f / Time.unscaledDeltaTime:F1}", CommandStatus.Info);
            CommandSystem.Log($"Frame Time: {Time.unscaledDeltaTime * 1000:F1}ms", CommandStatus.Info);
            CommandSystem.Log($"Time Scale: {Time.timeScale:F2}", CommandStatus.Info);
            CommandSystem.Log($"Fixed Delta Time: {Time.fixedDeltaTime:F4}s", CommandStatus.Info);
            CommandSystem.Log($"Render Pipeline: {UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.name ?? "Built-in"}", CommandStatus.Info);
        }

        [Command("version", "Mostra informações da versão do Commander")]
        public static void ShowVersion()
        {
            CommandSystem.Log("=== Commander Console ===", CommandStatus.Info);
            CommandSystem.Log("Versão: 2.0 (Dear ImGui)", CommandStatus.Success);
            CommandSystem.Log($"Unity: {Application.unityVersion}", CommandStatus.Info);
            CommandSystem.Log("Interface: UImGui (Dear ImGui)", CommandStatus.Info);
            CommandSystem.Log("Autor: Natteens", CommandStatus.Info);
        }

        [Command("test_colors", "Testa diferentes cores de log")]
        public static void TestColors()
        {
            CommandSystem.Log("✓ Teste de mensagem de sucesso", CommandStatus.Success);
            CommandSystem.Log("✗ Teste de mensagem de erro", CommandStatus.Error);
            CommandSystem.Log("! Teste de mensagem de aviso", CommandStatus.Warning);
            CommandSystem.Log("i Teste de mensagem informativa", CommandStatus.Info);
        }

        [Command("test_parameters", "Testa diferentes tipos de parâmetros")]
        public static void TestParameters(int numero = 42, float valorFloat = 3.14f, bool flag = true, string texto = "hello")
        {
            CommandSystem.Log($"Parâmetros testados:", CommandStatus.Info);
            CommandSystem.Log($"  - Número: {numero}", CommandStatus.Success);
            CommandSystem.Log($"  - Float: {valorFloat:F2}", CommandStatus.Success);
            CommandSystem.Log($"  - Flag: {flag}", CommandStatus.Success);
            CommandSystem.Log($"  - Texto: '{texto}'", CommandStatus.Success);
        }

        [Command("math_add", "Soma dois números")]
        public static void AddNumbers(float a, float b)
        {
            var result = a + b;
            CommandSystem.Log($"{a} + {b} = {result}", CommandStatus.Success);
        }

        [Command("math_multiply", "Multiplica dois números")]
        public static void MultiplyNumbers(float a, float b)
        {
            var result = a * b;
            CommandSystem.Log($"{a} × {b} = {result}", CommandStatus.Success);
        }

        [Command("teleport_camera", "Teleporta a câmera principal para uma posição")]
        public static void TeleportCamera(float x, float y, float z)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(x, y, z);
                CommandSystem.Log($"Câmera teleportada para: ({x:F1}, {y:F1}, {z:F1})", CommandStatus.Success);
            }
            else
            {
                CommandSystem.Log("Câmera principal não encontrada!", CommandStatus.Error);
            }
        }

        [Command("screenshot", "Captura uma screenshot")]
        public static void TakeScreenshot()
        {
            var filename = $"screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            var path = System.IO.Path.Combine(Application.persistentDataPath, filename);
            ScreenCapture.CaptureScreenshot(path);
            CommandSystem.Log($"Screenshot salva em: {path}", CommandStatus.Success);
        }
    }
}