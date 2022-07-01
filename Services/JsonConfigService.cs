using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ChatCommands.Services
{
    [AutoInject(typeof(IJsonConfigService), Lifetime = ServiceLifetime.Singleton)]
    public class JsonConfigService : IJsonConfigService
    {
        private ManualLogSource Logger { get; }
        private string KitsConfigPath { get; }
        private string PermissionsConfigPath { get; }
        private JsonSerializerOptions SerializationOptions { get; }

        public JsonConfigService(ManualLogSource logger)
        {
            Logger = logger;

            KitsConfigPath = Path.Combine(IJsonConfigService.ConfigPath, IJsonConfigService.Kits);
            PermissionsConfigPath = Path.Combine(IJsonConfigService.ConfigPath, IJsonConfigService.Permissions);

            SerializationOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
        }


        public void Initialize()
        {
            try
            {
                if (!Directory.Exists(IJsonConfigService.ConfigPath))
                    Directory.CreateDirectory(IJsonConfigService.ConfigPath);

                if (!File.Exists(KitsConfigPath))
                {
                    using var stream = File.Create(KitsConfigPath);
                }

                if (!File.Exists(PermissionsConfigPath))
                {
                    using var stream = File.Create(PermissionsConfigPath);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning("Unable to create json");
                Logger.LogError(e);
            }
        }

        public Dictionary<string, bool> LoadPermissions()
        {
            var permissions = new Dictionary<string, bool>();
            if (!File.Exists(PermissionsConfigPath))
                return permissions;

            using var hFile = File.OpenRead(PermissionsConfigPath);
            try
            {
                permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(hFile);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occurred trying to read the permissions json.");
                Logger.LogError(e);
            }
            return permissions;
        }

        public void SavePermissions(Dictionary<string, bool> permissions)
        {
            using var hFile = File.Open(PermissionsConfigPath, FileMode.Create, FileAccess.Write, FileShare.None);
            try
            {
                JsonSerializer.Serialize(hFile, permissions, SerializationOptions);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occurred trying to read the permissions json.");
                Logger.LogError(e);
            }
        }
    }
}
