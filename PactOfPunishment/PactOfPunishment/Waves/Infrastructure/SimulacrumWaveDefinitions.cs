using PactOfPunishment.Waves.Common;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Infrastructure
{
    public class SimulacrumWaveDefinitions
    {
        private readonly Dictionary<Type, ISimulacrumWaveDefinition> dictionary = new Dictionary<Type, ISimulacrumWaveDefinition>();

        public SimulacrumWaveDefinitions Add<T>()
            where T : ISimulacrumWaveDefinition, new()
        {
            this.dictionary.Add(typeof(T), new T());
            return this;
        }

        public T Get<T>()
            where T : ISimulacrumWaveDefinition
        {
            return (T)this.Get(typeof(T));
        }

        public Instance ForRun(Run run)
        {
            return new Instance(this, run);
        }

        public IPortableMiniBossWaveDefinition[] GetPortableMiniBossWaveDefinitions()
        {
            return this.dictionary.Values.OfType<IPortableMiniBossWaveDefinition>().ToArray();
        }

        private ISimulacrumWaveDefinition Get(Type type)
        {
            return this.dictionary[type];
        }

        public class Instance
        {
            private readonly SimulacrumWaveDefinitions waveDefinitions;

            private readonly Run run;

            private readonly Dictionary<Type, GameObject?> wavePrefabs = new Dictionary<Type, GameObject?>();

            internal Instance(SimulacrumWaveDefinitions waveDefinitions, Run run)
            {
                this.waveDefinitions = waveDefinitions;
                this.run = run;
            }

            public GameObject? Prefab<T>()
                where T : ISimulacrumWaveDefinition, new()
            {
                return this.Prefab(typeof(T));
            }

            public Instance Build()
            {
                foreach (var key in this.waveDefinitions.dictionary.Keys)
                {
                    this.Prefab(key);
                }

                return this;
            }

            public GameObject? TryGetWavePrefab(string waveDefinitionName)
            {
                var keyValuePair = this.waveDefinitions.dictionary.FirstOrDefault(x => string.Equals(waveDefinitionName, x.Value.Name, StringComparison.OrdinalIgnoreCase));

                if (keyValuePair.Key == null)
                {
                    return null;
                }

                return this.Prefab(keyValuePair.Key);
            }

            internal GameObject? Prefab(ISimulacrumWaveDefinition simulacrumWaveDefinition)
            {
                return simulacrumWaveDefinition.MakeWavePrefab(this.run);
            }

            private GameObject? Prefab(Type type)
            {
                if (!this.wavePrefabs.TryGetValue(type, out GameObject? obj))
                {
                    this.wavePrefabs[type] = obj = this.waveDefinitions.Get(type).MakeWavePrefab(this.run);
                }

                return obj;
            }
        }
    }
}