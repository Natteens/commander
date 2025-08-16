using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Commander
{
    public class ParameterParser
    {
        public object[] ParseParameters(Type[] parameterTypes, string[] args, int startIndex = 0)
        {
            var parameters = new object[parameterTypes.Length];
            var availableArgs = args.Skip(startIndex).ToArray();
            
            int argIndex = 0;
            
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                try
                {
                    if (parameterTypes[i] == typeof(Vector3))
                    {
                        // Vector3 consome 3 argumentos
                        if (argIndex + 2 < availableArgs.Length)
                        {
                            float x = ParseFloat(availableArgs[argIndex]);
                            float y = ParseFloat(availableArgs[argIndex + 1]);
                            float z = ParseFloat(availableArgs[argIndex + 2]);
                            parameters[i] = new Vector3(x, y, z);
                            argIndex += 3;
                        }
                        else
                        {
                            throw new ArgumentException($"Vector3 requires 3 values, got {availableArgs.Length - argIndex}");
                        }
                    }
                    else if (parameterTypes[i] == typeof(Vector2))
                    {
                        // Vector2 consome 2 argumentos
                        if (argIndex + 1 < availableArgs.Length)
                        {
                            float x = ParseFloat(availableArgs[argIndex]);
                            float y = ParseFloat(availableArgs[argIndex + 1]);
                            parameters[i] = new Vector2(x, y);
                            argIndex += 2;
                        }
                        else
                        {
                            throw new ArgumentException($"Vector2 requires 2 values, got {availableArgs.Length - argIndex}");
                        }
                    }
                    else
                    {
                        // Parâmetro normal consome 1 argumento
                        if (argIndex < availableArgs.Length)
                        {
                            parameters[i] = ConvertParameter(availableArgs[argIndex], parameterTypes[i]);
                            argIndex++;
                        }
                        else
                        {
                            parameters[i] = GetDefaultValue(parameterTypes[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleController.Log($"Erro ao processar parâmetro {i + 1}: {ex.Message}", 
                        CommandStatus.Error);
                    parameters[i] = GetDefaultValue(parameterTypes[i]);
                }
            }
            
            return parameters;
        }
        
        private object ConvertParameter(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return GetDefaultValue(targetType);
            
            try
            {
                // String
                if (targetType == typeof(string))
                    return value;
                
                // Bool
                if (targetType == typeof(bool))
                    return ParseBool(value);
                
                // Int
                if (targetType == typeof(int))
                    return ParseInt(value);
                
                // Float
                if (targetType == typeof(float))
                    return ParseFloat(value);
                
                // Color
                if (targetType == typeof(Color))
                    return ParseColor(value);
                
                // Fallback
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Não foi possível converter '{value}' para {targetType.Name}: {ex.Message}");
            }
        }
        
        private bool ParseBool(string value)
        {
            value = value.ToLower().Trim();
            
            return value switch
            {
                "true" or "1" or "on" or "yes" or "sim" => true,
                "false" or "0" or "off" or "no" or "nao" => false,
                _ => throw new ArgumentException($"Valor booleano inválido: {value}")
            };
        }
        
        private int ParseInt(string value)
        {
            value = value.Trim();
            
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                return result;
            
            throw new ArgumentException($"Número inteiro inválido: {value}");
        }
        
        private float ParseFloat(string value)
        {
            value = value.Trim().Replace(',', '.'); // Suporte a vírgula
            
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;
            
            throw new ArgumentException($"Número decimal inválido: {value}");
        }
        
        private Color ParseColor(string value)
        {
            value = value.Trim();
            
            // Cores por nome
            switch (value.ToLower())
            {
                case "red" or "vermelho": return Color.red;
                case "green" or "verde": return Color.green;
                case "blue" or "azul": return Color.blue;
                case "white" or "branco": return Color.white;
                case "black" or "preto": return Color.black;
                case "yellow" or "amarelo": return Color.yellow;
                case "cyan" or "ciano": return Color.cyan;
                case "magenta": return Color.magenta;
                case "gray" or "grey" or "cinza": return Color.gray;
            }
            
            // Hex
            if (value.StartsWith("#"))
            {
                if (ColorUtility.TryParseHtmlString(value, out Color color))
                    return color;
            }
            
            throw new ArgumentException($"Cor inválida: {value}");
        }
        
        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}