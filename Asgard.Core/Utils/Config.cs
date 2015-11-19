using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Utils
{
    /// <summary>
    /// Thread safe JSON config system
    /// </summary>
    public sealed class Config
    {
        private static string _configFile = "config.json";
        private static bool _loaded = false;
        private static object _lockObject = new object();
        private static JObject _jsonObject = null;

        #region Private Static
        /// <summary>
        /// lazy load config data if consumer doesn't override
        /// </summary>
        private static void EnsureLoaded()
        {
            if (!_loaded) //quick optimization around locking once loaded
            {
                lock (_lockObject)
                {
                    if (!_loaded) //check for race condition since first check was outside
                                  //lock
                    {
                        _loaded = true;
                        Load();
                    }
                }
            }
        }

        #endregion

        #region Public Static
        /// <summary>
        /// Overrides config file with consumer supplied file name
        /// </summary>
        /// <param name="config"></param>
        public static void Load(string config = "")
        {
            if (!string.IsNullOrEmpty(config))
            {
                _configFile = config;
            }

            if (!File.Exists(_configFile))
            {
                var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase);
                _configFile = Path.Combine(basePath, _configFile);

                if (!File.Exists(_configFile))
                {
                    _jsonObject = new JObject();
                    return;
                }
            }

            var rawJson = File.ReadAllText(_configFile);
            _jsonObject = JObject.Parse(rawJson);

            if (_jsonObject == null)
            {
                throw new InvalidDataException("bad config data");
            }
        }

        public static string GetString(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException();
            }

            EnsureLoaded();

            JToken token = null;
            var tokenFound = _jsonObject.TryGetValue(key, out token);
            if (!tokenFound)
            {
                return String.Empty;
            }

            return token.ToString();
        }

        public static int GetInt(string key)
        {
            var result = GetString(key);
            Int32 intResult;
            bool parseResult = Int32.TryParse(result, out intResult);
            if (!parseResult)
            {
                return 0;
            }

            return intResult;
        }

        public static T Get<T>(string key)
        {
            var json = GetString(key);
            T result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }
        #endregion

    }
}
