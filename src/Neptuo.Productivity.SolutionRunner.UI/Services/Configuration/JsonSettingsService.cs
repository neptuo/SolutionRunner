﻿using Neptuo.Activators;
using Neptuo.Collections.Specialized;
using Neptuo.Formatters;
using Neptuo.PresentationModels;
using Neptuo.PresentationModels.TypeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptuo.Productivity.SolutionRunner.Services.Configuration
{
    public class JsonSettingsService : ISettingsService
    {
        private static readonly CompositeModelFormatter formatter = new CompositeModelFormatter(
            type => Activator.CreateInstance(type),
            Factory.Getter(() => new JsonCompositeStorage())
        );

        private static readonly IModelDefinition modelDefinition = new TypeModelDefinitionCollection().Get<JsonSettings>();

        private readonly Func<string> filePathGetter;

        public JsonSettingsService(Func<string> filePathGetter)
        {
            Ensure.NotNull(filePathGetter, "filePathGetter");
            this.filePathGetter = filePathGetter;
        }

        public async Task<JsonSettings> LoadInternalAsync()
        {
            JsonSettings settings = null;

            string filePath = filePathGetter();
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath);
                settings = await formatter.DeserializeAsync<JsonSettings>(fileContent);
            }
            else
            {
                settings = new JsonSettings();
            }

            return settings;
        }

        public async Task<ISettings> LoadAsync()
        {
            return await LoadInternalAsync();
        }

        public async Task<IKeyValueCollection> LoadRawAsync()
        {
            JsonSettings settings = await LoadInternalAsync();
            ReflectionModelValueProvider<JsonSettings> valueProvider = new ReflectionModelValueProvider<JsonSettings>(settings);
            return new ModelValueCollection(valueProvider, modelDefinition);
        }

        public async Task SaveAsync(ISettings settings)
        {
            JsonSettings target = (JsonSettings)settings;
            string filePath = filePathGetter();
            string fileContent = await formatter.SerializeAsync(target);
            File.WriteAllText(filePath, fileContent);
        }
    }
}