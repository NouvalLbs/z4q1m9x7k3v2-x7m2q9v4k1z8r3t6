using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ProjectSMP.Core
{
    public enum Language {
        EN,
        ID
    }

    public static class LocalizationManager {
        private const string FilePath = "Localization.json";
        private const string FallbackKey = "EN";

        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _data = new();

        public static void Load() {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException($"Localization file not found: {FilePath}");

            var json = File.ReadAllText(FilePath);
            _data = JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            >(json) ?? new();
        }

        public static string Get(Language lang, string section, string key) {
            var langKey = lang.ToString();

            if (_data.TryGetValue(langKey, out var sections) &&
                sections.TryGetValue(section, out var keys) &&
                keys.TryGetValue(key, out var value))
                return value;

            if (_data.TryGetValue(FallbackKey, out var fallbackSections) &&
                fallbackSections.TryGetValue(section, out var fallbackKeys) &&
                fallbackKeys.TryGetValue(key, out var fallbackValue))
                return fallbackValue;

            return $"[{section}.{key}]";
        }

        public static string Get(Language lang, string section, string key, params object[] args) {
            var raw = Get(lang, section, key);
            try {
                return string.Format(raw, args);
            } catch {
                return raw;
            }
        }
    }
}