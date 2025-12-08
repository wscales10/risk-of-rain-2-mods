using BepInEx.Logging;
using CsvHelper;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PerEnvironmentElites
{
    internal class CsvWeightGetter : IWeightGetter
    {
        private readonly Dictionary<string, SceneInfo> cache = new Dictionary<string, SceneInfo>();

        private readonly ManualLogSource logger;

        public CsvWeightGetter(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public decimal GetWeight(EliteDef eliteDef, SceneDef env)
        {
            if (!this.cache.TryGetValue(env.cachedName, out var sceneInfo))
            {
                sceneInfo = this.cache.Values.FirstOrDefault(x => x.Regex.IsMatch(env.cachedName));
            }

            if (sceneInfo is null)
            {
                this.logger.LogWarning($"No data found for scene {env.cachedName}");
                return 1;
            }

            try
            {
                return sceneInfo.GetWeight(eliteDef);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Error getting elite weight: {ex}");
                return 1;
            }
        }

        public void Init()
        {
            this.cache.Clear();
            StreamReader reader;

            try
            {
                var fileInfo = new FileInfo(Assembly.GetAssembly(this.GetType()).Location);
                reader = new StreamReader(System.IO.Path.Combine(fileInfo.DirectoryName, "eliteWeights.csv"));
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Error loading eliteWeights.csv: {ex}");
                return;
            }

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord ?? throw new FormatException("No headers in CSV file.");

            bool isFirst = true;

            while (csv.Read())
            {
                string sceneName = csv.GetField("SceneName") ?? throw new FormatException("Missing scene name.");
                var dictionary = new Dictionary<string, decimal>();

                foreach (var fieldName in headers)
                {
                    var match = Regex.Match(fieldName, "(.*)Weight");

                    if (match.Success)
                    {
                        if (decimal.TryParse(csv.GetField(fieldName), out var weight))
                        {
                            string key = match.Groups[1].Value;
                            dictionary[key] = weight;
                            this.logger.LogDebug($"Weight of {key} elite on {sceneName} = {weight}");
                        }
                    }
                    else if (isFirst && !fieldName.EndsWith("Name"))
                    {
                        this.logger.LogWarning($"Unrecognised field name '{fieldName}'.");
                    }
                }

                this.cache.Add(sceneName, new SceneInfo(dictionary, new Regex($@"^(IT)?{Regex.Escape(sceneName)}(SIMPLE)?\d*$")));
                isFirst = false;
            }
        }

        private sealed class SceneInfo
        {
            private static readonly Regex eliteDefRegex = new Regex(@"^ed(?<EliteType>\w+?)(Honor)?$");

            private readonly Dictionary<string, decimal> dictionary;

            public SceneInfo(Dictionary<string, decimal> dictionary, Regex regex)
            {
                this.dictionary = dictionary;
                this.Regex = regex;
            }

            public Regex Regex { get; }

            public decimal GetWeight(EliteDef eliteDef)
            {
                var match = eliteDefRegex.Match(eliteDef.name);

                if (!match.Success)
                {
                    throw new ArgumentOutOfRangeException(nameof(eliteDef), eliteDef, $"Elite Definition name '{eliteDef.name}' is not in the expected format.");
                }

                return this.dictionary[match.Groups["EliteType"].Value];
            }
        }
    }
}