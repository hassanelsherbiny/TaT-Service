﻿using PowerStore.Domain.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PowerStore.Core.Data
{
    /// <summary>
    /// Manager of data settings (connection string)
    /// </summary>
    public partial class DataSettingsManager
    {
        protected const char separator = ':';
        protected const string filename = "Settings.txt";

        private DataSettings _dataSettings;

        protected string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parse settings
        /// </summary>
        /// <param name="text">Text of settings file</param>
        /// <returns>Parsed data settings</returns>
        protected virtual DataSettings ParseSettings(string text)
        {
            var shellSettings = new DataSettings();
            if (string.IsNullOrEmpty(text))
                return shellSettings;

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                    settings.Add(str);
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(separator);
                if (separatorIndex == -1)
                {
                    continue;
                }
                var key = setting.Substring(0, separatorIndex).Trim();
                var value = setting.Substring(separatorIndex + 1).Trim();
                if (!string.IsNullOrEmpty(key))
                    key = RemoveSpecialCharacters(key);

                switch (key)
                {
                    case "DataProvider":
                        shellSettings.DataProvider = value;
                        break;
                    case "DataConnectionString":
                        shellSettings.DataConnectionString = value;
                        break;
                    default:
                        shellSettings.RawDataSettings.Add(key, value);
                        break;
                }
            }

            return shellSettings;
        }

        /// <summary>
        /// Convert data settings to string representation
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Text</returns>
        protected virtual string ComposeSettings(DataSettings settings)
        {
            if (settings == null)
                return "";

            return string.Format("DataProvider: {0}{2}DataConnectionString: {1}{2}",
                                 settings.DataProvider,
                                 settings.DataConnectionString,
                                 Environment.NewLine
                );
        }

        /// <summary>
        /// Load settings
        /// </summary>
        public virtual DataSettings LoadSettings(string filePath = null, bool reloadSettings = false)
        {
            if (!reloadSettings && _dataSettings != null)
                return _dataSettings;

            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(CommonHelper.MapPath("~/App_Data/"), filename);

            if (!File.Exists(filePath))
                return new DataSettings();

            var text = File.ReadAllText(filePath);
            _dataSettings = ParseSettings(text);
            return _dataSettings;

        }

        /// <summary>
        /// Save settings to a file
        /// </summary>
        /// <param name="settings"></param>
        public virtual async Task SaveSettings(DataSettings settings)
        {
            var filePath = Path.Combine(CommonHelper.MapPath("~/App_Data/"), filename);
            if (!File.Exists(filePath))
            {
                using FileStream fs = File.Create(filePath);
            }
            var text = ComposeSettings(settings);
            await File.WriteAllTextAsync(filePath, text);
        }
    }
}
