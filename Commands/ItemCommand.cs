using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Models;
using ChatCommands.Utils;
using System.Linq;

namespace ChatCommands.Commands
{
    [ChatCommand("item", "item $name <$discriminator | $amount> <$amount> ", "Add an item")]
    public class ItemCommand : BaseChatCommand
    {
        private IItemService ItemService { get; }

        public ItemCommand(ManualLogSource logger, IItemService itemService) : base(logger)
        {
            ItemService = itemService;
        }

        public override CommandResult Handle(CommandContext ctx)
        {
            if (!ctx.Args.TryDequeue(out var itemName))
            {
                Logger.LogWarning($"Could not retrieve `{nameof(itemName)}`. {ctx.GetArgumentsForLog()}");
                return CommandResult.MissingArguments;
            }

            var items = ItemService.FindItem(itemName);
            if (items.Count == 0)
            {
                ctx.EventUser.SendMessage($"No item with the name `{itemName}` could be found.");
                return CommandResult.Success;
            }

            if (items.Count == 1)
            {
                if (!ctx.Args.TryDequeueInt(out var strQuantity, out var quantity) && !string.IsNullOrWhiteSpace(strQuantity))
                    ctx.EventUser.SendMessage($"The value `{strQuantity}` could not be parsed as an integer.");

                quantity = System.Math.Max(1, quantity);
                ctx.EventUser.SendMessage(
                    ItemService.AddItemToInventory(items.First(), ctx.EntityManager, ctx.CharacterEntity, quantity)
                        ? $"Item Added {quantity}x `{itemName}`."
                        : $"Could not add {quantity}x `{itemName}` to your inventory.");
                return CommandResult.Success;
            }

            if (items.Count > 1)
            {
                if (!ctx.Args.TryDequeue(out var nextValue))
                {
                    // No value found
                    ctx.EventUser.SendMessage($"More than one item was found for `{itemName}`. Please select the correct item and use the command again.");

                    for(var i = 0; i < items.Count; i++)
                        ctx.EventUser.SendMessage($"[{(char)('a' + i)}] {items[i].Name}");

                    return CommandResult.Success;
                }

                if (nextValue.Length != 1)
                {
                    ctx.EventUser.SendMessage($"More than one item was found for `{itemName}`. Please select an item by including it's letter (a - {(char)('a' + items.Count)}) and use the command again.");
                    return CommandResult.InvalidArguments;
                }

                var index = System.Math.Clamp(nextValue[0] - 'a', 0, items.Count - 1);
                if (!ctx.Args.TryDequeueInt(out var strQuantity, out var quantity) && !string.IsNullOrWhiteSpace(strQuantity))
                    ctx.EventUser.SendMessage($"The value `{strQuantity}` could not be parsed as an integer.");

                quantity = System.Math.Max(1, quantity);
                ctx.EventUser.SendMessage(
                    ItemService.AddItemToInventory(items[index], ctx.EntityManager, ctx.CharacterEntity, quantity)
                        ? $"Item Added {quantity}x `{itemName}`."
                        : $"Could not add {quantity}x `{itemName}` to your inventory.");
                return CommandResult.Success;
            }

            ctx.EventUser.SendMessage($"Something impossible happened with `{itemName}`.");
            return CommandResult.Success;
        }
    }
}
