﻿using STL_Showcase.Data.Config;
using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Logic.Localization
{
    public class Loc
    {

        private Dictionary<string, Dictionary<string, string>> LoadedTexts;

        private static Loc _Ins;
        public static Loc Ins {
            get
            {
                if (_Ins == null)
                    _Ins = new Loc();
                return _Ins;
            }
            private set
            {
                _Ins = value;
            }
        }
        public const string DefaultLanguage = "en";

        public string[] LoadedLanguages { get; private set; }
        public string CurrentLanguage { get; private set; }

        public Action<string> OnLanguageChanged;

        private Loc()
        {
            _Ins = this;
            CultureInfo ci = CultureInfo.CurrentUICulture;

            LoadTexts();

            IUserSettings settings = DefaultFactory.GetDefaultUserSettings();
            CurrentLanguage = settings.GetSettingString(Shared.Enums.UserSettingEnum.Language);
            if (!LoadedLanguages.Contains(CurrentLanguage))
            {
                CurrentLanguage = DefaultLanguage;
            }
        }

        public static string GetText(string key, string forLanguage = "")
        {
            return Ins._GetText(key, forLanguage);
        }
        private string _GetText(string key, string forLanguage = "")
        {
            Dictionary<string, string> translatedTexts;

            if (LoadedTexts.TryGetValue(string.IsNullOrWhiteSpace(forLanguage) ? CurrentLanguage : forLanguage, out translatedTexts))
            {
                if (translatedTexts.TryGetValue(key, out string translatedText))
                {
                    return translatedText;
                }
            }

            if (LoadedTexts.TryGetValue(DefaultLanguage, out translatedTexts))
            {
                if (translatedTexts.TryGetValue(key, out string translatedText))
                {
                    return translatedText;
                }
            }

            return $"%{key}";
        }

        public void SetLanguage(string newLanguage)
        {
            CultureInfo ci = CultureInfo.CurrentUICulture;

            if (!LoadedLanguages.Contains(newLanguage))
                return;

            IUserSettings settings = DefaultFactory.GetDefaultUserSettings();
            settings.SetSettingString(Shared.Enums.UserSettingEnum.Language, newLanguage);

            CurrentLanguage = newLanguage;
            OnLanguageChanged?.Invoke(newLanguage);
        }

        private void LoadTexts()
        {
            string appPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Loc)).Location);
            string localizationPath = Path.Combine(appPath, "Loc");
            string[] localizationFiles = Directory.GetFiles(localizationPath, "*.txt");

            HashSet<string> supportedLanguages = new HashSet<string>(CultureInfo.GetCultures(CultureTypes.NeutralCultures).Select(ci => ci.TwoLetterISOLanguageName));

            LoadedTexts = new Dictionary<string, Dictionary<string, string>>();

            for (int i = 0; i < localizationFiles.Length; i++)
            {
                string languageName = Path.GetFileNameWithoutExtension(localizationFiles[i]);
                if (!supportedLanguages.Contains(languageName))
                {
                    throw new CultureNotFoundException($"ERROR Loading language file: file {localizationFiles[i]} is not a supported language.");
                }

                Dictionary<string, string> newTexts = new Dictionary<string, string>();

                foreach (string line in File.ReadLines(localizationFiles[i], Encoding.GetEncoding(1252))) // Set high-ANSI encoding (found at: https://stackoverflow.com/a/37145016)
                {
                    int separationIndex = line.IndexOf(' ');
                    if (!newTexts.ContainsKey(line.Substring(0, separationIndex)))
                        newTexts.Add(line.Substring(0, separationIndex), line.Substring(separationIndex + 1).Replace(@"\n", "\n"));
                }

                LoadedTexts.Add(languageName, newTexts);
            }

            LoadedLanguages = LoadedTexts.Keys.ToArray();
        }
    }
}
