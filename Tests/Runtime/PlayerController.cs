using System.Linq;
using UnityEngine;

namespace Commander.Tests.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private float health = 100f;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool godMode = false;
        
        private Vector3 originalPosition;
        
        private void Start()
        {
            originalPosition = transform.position;
        }
        
        // Teleporte simples - aceita 3 números separados
        [Command("teleport", "Teleportar jogador. Uso: teleport x y z")]
        public void TeleportPlayer(Vector3 position)
        {
            transform.position = position;
            ConsoleController.Log($"Jogador teleportado para {position}", 
                CommandStatus.Success);
        }
        
        // Mover objeto por nome
        [Command("move", "Mover objeto. Uso: move NomeDoObjeto x y z")]
        public static void MoveObject(string targetName, Vector3 position)
        {
            GameObject target = GameObject.Find(targetName);
            if (target == null)
            {
                // Busca parcial se não encontrar exato
                var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                target = allObjects.FirstOrDefault(go => 
                    go.name.ToLower().Contains(targetName.ToLower()));
            }
            
            if (target == null)
            {
                ConsoleController.Log($"Objeto '{targetName}' não encontrado", 
                    CommandStatus.Error);
                return;
            }
            
            target.transform.position = position;
            ConsoleController.Log($"'{target.name}' movido para {position}", 
                CommandStatus.Success);
        }
        
        // Escalar objeto
        [Command("scale", "Escalar objeto. Uso: scale NomeDoObjeto x y z")]
        public static void ScaleObject(string targetName, Vector3 scale)
        {
            GameObject target = FindObjectByName(targetName);
            if (target == null)
            {
                ConsoleController.Log($"Objeto '{targetName}' não encontrado", 
                    CommandStatus.Error);
                return;
            }
            
            target.transform.localScale = scale;
            ConsoleController.Log($"'{target.name}' escalado para {scale}", 
                CommandStatus.Success);
        }
        
        // Rotacionar objeto
        [Command("rotate", "Rotacionar objeto. Uso: rotate NomeDoObjeto x y z")]
        public static void RotateObject(string targetName, Vector3 rotation)
        {
            GameObject target = FindObjectByName(targetName);
            if (target == null)
            {
                ConsoleController.Log($"Objeto '{targetName}' não encontrado", 
                    CommandStatus.Error);
                return;
            }
            
            target.transform.rotation = Quaternion.Euler(rotation);
            ConsoleController.Log($"'{target.name}' rotacionado para {rotation}°", 
                CommandStatus.Success);
        }
        
        // Alterar cor
        [Command("setcolor", "Alterar cor do objeto. Uso: setcolor NomeDoObjeto CorOuHex")]
        public static void SetObjectColor(string targetName, Color color)
        {
            GameObject target = FindObjectByName(targetName);
            if (target == null)
            {
                ConsoleController.Log($"Objeto '{targetName}' não encontrado", 
                    CommandStatus.Error);
                return;
            }
            
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                ConsoleController.Log($"'{target.name}' não tem componente Renderer", 
                    CommandStatus.Error);
                return;
            }
            
            renderer.material.color = color;
            ConsoleController.Log($"Cor de '{target.name}' alterada para {color}", 
                CommandStatus.Success);
        }
        
        // Método auxiliar para busca de objetos
        private static GameObject FindObjectByName(string name)
        {
            // Busca exata primeiro
            GameObject target = GameObject.Find(name);
            if (target != null) return target;
            
            // Busca parcial se não encontrar
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            return allObjects.FirstOrDefault(go => 
                go.name.ToLower().Contains(name.ToLower()));
        }
        
        // Comandos simples existentes
        [Command("heal", "Curar jogador. Uso: heal [quantidade]")]
        public void HealPlayer(float amount = 50f)
        {
            float oldHealth = health;
            health = Mathf.Min(health + amount, maxHealth);
            
            ConsoleController.Log($"Jogador curado de {oldHealth:F1} para {health:F1} HP", 
                CommandStatus.Success);
        }
        
        [Command("god", "Alternar modo deus")]
        public void ToggleGodMode()
        {
            godMode = !godMode;
            ConsoleController.Log($"Modo deus: {(godMode ? "ATIVADO" : "DESATIVADO")}", 
                CommandStatus.Success);
        }
        
        [Command("spawn", "Voltar ao ponto inicial")]
        public void TeleportToSpawn()
        {
            transform.position = originalPosition;
            ConsoleController.Log("Jogador teleportado para o spawn", 
                CommandStatus.Success);
        }
        
        [Command("stats", "Mostrar estatísticas do jogador")]
        public void ShowStats()
        {
            ConsoleController.Log("=== ESTATÍSTICAS DO JOGADOR ===", CommandStatus.Info);
            ConsoleController.Log($"Vida: {health:F1}/{maxHealth:F1}", CommandStatus.Info);
            ConsoleController.Log($"Velocidade: {speed:F1}", CommandStatus.Info);
            ConsoleController.Log($"Posição: {transform.position}", CommandStatus.Info);
            ConsoleController.Log($"Modo Deus: {(godMode ? "ATIVO" : "INATIVO")}", CommandStatus.Info);
        }
    }
}