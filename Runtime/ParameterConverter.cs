using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Commander
{
    /// <summary>
    /// Manipula conversão de parâmetros para execução de comandos
    /// </summary>
    public sealed class ParameterConverter
    {
        private static readonly Dictionary<Type, Func<string, object>> Converters = new()
        {
            { typeof(bool), ConvertBool },
            { typeof(byte), s => byte.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(sbyte), s => sbyte.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(short), s => short.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(ushort), s => ushort.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(int), s => int.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(uint), s => uint.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(long), s => long.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(ulong), s => ulong.Parse(s, CultureInfo.InvariantCulture) },
            { typeof(float), ConvertFloat },
            { typeof(double), s => double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture) },
            { typeof(decimal), s => decimal.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture) },
            { typeof(string), s => s },
            { typeof(Vector2), ConvertUnityVector2 },
            { typeof(Vector3), ConvertUnityVector3 },
            { typeof(Color), ConvertColor }
        };

        public object Convert(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return GetDefaultValue(targetType);

            if (targetType.IsEnum)
                return ConvertEnum(value, targetType);

            if (IsArrayType(targetType))
                return ConvertArray(value, targetType);

            if (IsListType(targetType))
                return ConvertList(value, targetType);

            if (IsDictionaryType(targetType))
                return ConvertDictionary(value, targetType);

            if (Converters.TryGetValue(targetType, out var converter))
                return converter(value);

            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static object ConvertBool(string value)
        {
            var lower = value.ToLower().Trim();
            return lower is "true" or "1" or "on" or "yes";
        }

        private static object ConvertFloat(string value)
        {
            return float.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        private static object ConvertUnityVector2(string value)
        {
            var parts = value.Split(',', ' ', ';').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            if (parts.Length >= 2)
            {
                var x = float.Parse(parts[0].Replace(',', '.'), CultureInfo.InvariantCulture);
                var y = float.Parse(parts[1].Replace(',', '.'), CultureInfo.InvariantCulture);
                return new Vector2(x, y);
            }
            return Vector2.zero;
        }

        private static object ConvertUnityVector3(string value)
        {
            var parts = value.Split(',', ' ', ';').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            if (parts.Length >= 3)
            {
                var x = float.Parse(parts[0].Replace(',', '.'), CultureInfo.InvariantCulture);
                var y = float.Parse(parts[1].Replace(',', '.'), CultureInfo.InvariantCulture);
                var z = float.Parse(parts[2].Replace(',', '.'), CultureInfo.InvariantCulture);
                return new Vector3(x, y, z);
            }
            return Vector3.zero;
        }

        private static object ConvertColor(string value)
        {
            var lower = value.ToLower().Trim();

            var namedColors = new Dictionary<string, Color>
            {
                { "red", Color.red }, { "green", Color.green }, { "blue", Color.blue },
                { "white", Color.white }, { "black", Color.black }, { "yellow", Color.yellow },
                { "cyan", Color.cyan }, { "magenta", Color.magenta }, { "gray", Color.gray }
            };

            if (namedColors.TryGetValue(lower, out var namedColor))
                return namedColor;

            if (value.StartsWith("#") && ColorUtility.TryParseHtmlString(value, out var hexColor))
                return hexColor;

            return Color.white;
        }

        private static object ConvertEnum(string value, Type enumType)
        {
            return Enum.Parse(enumType, value, true);
        }

        private static bool IsArrayType(Type type) => type.IsArray;
        private static bool IsListType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        private static bool IsDictionaryType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        private static object ConvertArray(string value, Type arrayType)
        {
            var elementType = arrayType.GetElementType();
            var parts = value.Split(',', ';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

            var array = Array.CreateInstance(elementType!, parts.Length);
            var converter = new ParameterConverter();

            for (var i = 0; i < parts.Length; i++)
                array.SetValue(converter.Convert(parts[i], elementType), i);

            return array;
        }

        private static object ConvertList(string value, Type listType)
        {
            var elementType = listType.GetGenericArguments()[0];
            var parts = value.Split(',', ';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

            var list = (IList)Activator.CreateInstance(listType);
            var converter = new ParameterConverter();

            foreach (var part in parts)
                list.Add(converter.Convert(part, elementType));

            return list;
        }

        private static object ConvertDictionary(string value, Type dictionaryType)
        {
            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];
            var pairs = value.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p));

            var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);
            var converter = new ParameterConverter();

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split(':');
                if (keyValue.Length == 2)
                {
                    var key = converter.Convert(keyValue[0].Trim(), keyType);
                    var val = converter.Convert(keyValue[1].Trim(), valueType);
                    dictionary.Add(key, val);
                }
            }

            return dictionary;
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}