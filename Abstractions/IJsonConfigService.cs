using System.Collections.Generic;

namespace ChatCommands.Abstractions;

internal interface IJsonConfigService
{
    internal const string ConfigPath = "BepInEx/config/ChatCommands";
    internal const string Kits = "kits.json";
    internal const string Permissions = "permissions.json";

    /// <summary>
    /// Initialize configuration files
    /// </summary>
    void Initialize();

    Dictionary<string, bool> LoadPermissions();
    void SavePermissions(Dictionary<string, bool> permissions);
}