using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Models;
using ChatCommands.Utils;
using ProjectM;
using Wetstone.API;

namespace ChatCommands.Commands
{
    [ChatCommand("blood", Usage = "blood <Type> [<Quality>] [<Value>]",
        Description = "Sets your current Blood Type, Quality and Value")]
    public class Blood : IChatCommand
    {
        public CommandResult Handle(CommandContext ctx)
        {
            if (ctx.Args.Count != 0)
            {
                try
                {
                    CommandHelper.BloodType type = CommandHelper.BloodType.Frailed;
                    float quality = 100;
                    float value = 100;

                    if (ctx.Args.Count >= 1)
                        type = CommandHelper.GetBloodTypeFromName(ctx.Args.Dequeue());
                    if (ctx.Args.Count >= 2 && float.TryParse(ctx.Args.Dequeue(), out var fQuality))
                        quality = System.Math.Clamp(fQuality, 0, 100);
                    if (ctx.Args.Count >= 3 && float.TryParse(ctx.Args.Dequeue(), out var fValue))
                        value = fValue;

                    var component = ctx.EntityManager.GetComponentData<ProjectM.Blood>(ctx.Event.SenderCharacterEntity);
                    component.BloodType = new PrefabGUID((int) type);
                    component.Quality = quality;
                    component.Value = value;
                    if (component.ShowBloodHUD.Value) component.ShowBloodHUD.Value = false;
                    ctx.EntityManager.SetComponentData(ctx.Event.SenderCharacterEntity, component);
                    ctx.Event.User.SendSystemMessage(
                        $"Changed Blood Type to <color=#ffff00ff>{type}</color> with <color=#ffff00ff>{quality}</color>% quality");
                }
                catch
                {
                    return CommandResult.InvalidArguments;
                }

                return CommandResult.Success;
            }

            return CommandResult.MissingArguments;
        }
    }
}