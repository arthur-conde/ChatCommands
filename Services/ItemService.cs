using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Models;
using ProjectM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using StunLocalization;
using Unity.Entities;
using Wetstone.API;

namespace ChatCommands.Services
{
    [AutoInject(typeof(IItemService))]
    public class ItemService : IItemService
    {
        public ItemService(ManualLogSource logger)
        {
            Logger = logger;

            Cache = new ConcurrentDictionary<string, int>();
        }

        private ManualLogSource Logger { get; }

        private string[] ItemBlacklist = new[]
            {"Item_VBloodSource", "GM_Unit_Creature_Base", "Item_Cloak_ShadowPriest"};


        private ConcurrentDictionary<string, int> Cache { get; }

        public IReadOnlyList<SimpleItem> FindItem(string needle)
        {
            var results = new List<SimpleItem>();

            var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
            var dataRegistry = gameDataSystem.ManagedDataRegistry;

            foreach (var kvp in gameDataSystem.ItemHashLookupMap)
            {
                try
                {
                    var itemData = dataRegistry.GetOrDefault<ManagedItemData>(kvp.Key);
                    if (ItemBlacklist.Any(itemData.PrefabName.StartsWith)) continue;
                    if (itemData.Name.IsEmpty || itemData.Name.ToString() is not {} itemName|| !string.Equals(itemName, needle, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var simpleItem = new SimpleItem(kvp.Key.GuidHash, itemName);
                    results.Add(simpleItem);
                }
                catch
                {

                }
            }

            return results;
        }

        public SimpleItem GetItem(string needle)
        {
            if (!Cache.ContainsKey(needle) || !Cache.TryGetValue(needle, out var itemId))
                return null;

            var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
            var dataRegistry = gameDataSystem.ManagedDataRegistry;
            try
            {
                var itemData = dataRegistry.GetOrDefault<ManagedItemData>(new PrefabGUID(itemId));
                return new SimpleItem(itemId, itemData.Name.ToString());
            }
            catch (Exception e)
            {
                Logger.LogError($"Unable to retrieve an item with name `{needle}`");
                Logger.LogError(e);
                return null;
            }
        }

        public unsafe bool AddItemToInventory(SimpleItem item, EntityManager entityManager, Entity target,
            int amount = 1)
        {
            var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
            try
            {
                var bytes = stackalloc byte[Marshal.SizeOf<FakeNull>()];
                var bytePtr = new IntPtr(bytes);
                Marshal.StructureToPtr<FakeNull>(new()
                {
                    value = 7,
                    has_value = true
                }, bytePtr, false);
                var boxedBytePtr = IntPtr.Subtract(bytePtr, 0x10);
                var hack = new Il2CppSystem.Nullable<int>(boxedBytePtr);
                return InventoryUtilitiesServer.TryAddItem(entityManager, gameDataSystem.ItemHashLookupMap, target,
                    item.ToGuid(), amount, out _, out _, default, hack);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

            return false;
        }
    }
}
