using UnityEngine;

namespace Commander.Tests.Runtime
{
    /// <summary>
    /// Exemplo de GameManager com comandos de teste e administração
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private int score = 0;
        [SerializeField] private int level = 1;
        
        private void Start()
        {
            ConsoleController.Log("GameManager initialized", CommandStatus.Info);
        }
        
        // ===========================================
        // COMANDOS DE ADMINISTRAÇÃO DO JOGO
        // ===========================================
        
        [Command("pause", "Pause/unpause the game")]
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            
            ConsoleController.Log($"Game {(isPaused ? "PAUSED" : "RESUMED")}", 
                CommandStatus.Success);
        }
        
        [Command("setscore", "Set player score")]
        public void SetScore(int newScore)
        {
            score = Mathf.Max(newScore, 0);
            ConsoleController.Log($"Score set to {score}", 
                CommandStatus.Success);
        }
        
        [Command("addscore", "Add points to score")]
        public void AddScore(int points = 100)
        {
            score += points;
            ConsoleController.Log($"Added {points} points. Total score: {score}", 
                CommandStatus.Success);
        }
        
        [Command("setlevel", "Set current level")]
        public void SetLevel(int newLevel)
        {
            level = Mathf.Max(newLevel, 1);
            ConsoleController.Log($"Level set to {level}", 
                CommandStatus.Success);
        }
        
        [Command("nextlevel", "Go to next level")]
        public void NextLevel()
        {
            level++;
            ConsoleController.Log($"Advanced to level {level}", 
                CommandStatus.Success);
        }
        
        [Command("gamestats", "Show game statistics")]
        public void ShowGameStats()
        {
            ConsoleController.Log("=== GAME STATS ===", CommandStatus.Info);
            ConsoleController.Log($"Score: {score}", CommandStatus.Info);
            ConsoleController.Log($"Level: {level}", CommandStatus.Info);
            ConsoleController.Log($"Paused: {(isPaused ? "YES" : "NO")}", CommandStatus.Info);
            ConsoleController.Log($"Time Scale: {Time.timeScale:F2}", CommandStatus.Info);
        }
        
        [Command("reset", "Reset game to initial state")]
        public void ResetGame()
        {
            score = 0;
            level = 1;
            isPaused = false;
            Time.timeScale = 1f;
            
            ConsoleController.Log("Game reset to initial state", 
                CommandStatus.Success);
        }
        
        // Comandos estáticos para testes
        [Command("testall", "Run all test commands")]
        public static void RunAllTests()
        {
            ConsoleController.Log("=== RUNNING ALL TESTS ===", CommandStatus.Info);
            
            // Testa comandos básicos
            ConsoleController.ExecuteCommand("help");
            ConsoleController.ExecuteCommand("list");
            ConsoleController.ExecuteCommand("fps");
            ConsoleController.ExecuteCommand("memory");
            
            ConsoleController.Log("All tests completed", CommandStatus.Success);
        }
        
        [Command("benchmark", "Run performance benchmark")]
        public static void RunBenchmark()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Executa vários comandos
            for (int i = 0; i < 100; i++)
            {
                ConsoleController.ExecuteCommand("list");
            }
            
            stopwatch.Stop();
            ConsoleController.Log($"Benchmark: 100 commands executed in {stopwatch.ElapsedMilliseconds}ms", 
                CommandStatus.Info);
        }
    }
}