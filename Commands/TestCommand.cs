using System;
using System.Reflection;
using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Models;
using ChatCommands.Utils;
using ProjectM;
using Wetstone.API;

namespace ChatCommands.Commands
{
    [ChatCommand("debug", Usage = "debug prop <value|reset>", Description = "Fiddles with things")]
    public class TestCommand : IChatCommand
    {
        private ManualLogSource Logger { get; }
        private IExpressionCache<UnitStats> UnitStatsCache { get; }

        public TestCommand(ManualLogSource logger, IExpressionCache<UnitStats> unitStatsCache)
        {
            Logger = logger;
            UnitStatsCache = unitStatsCache;
        }

        public CommandResult Handle(CommandContext ctx)
        {
            if (!ctx.Args.TryDequeue(out var subcommand))
            {
                Logger.LogWarning($"Could not retrieve `{nameof(subcommand)}`. {ctx.GetArgumentsForLog()}");
                return CommandResult.MissingArguments;
            }

            switch (subcommand.ToLowerInvariant())
            {
                case "get":
                {
                    return GetProperty(ctx);
                }
                case "set":
                {
                    return SetProperty(ctx);
                }
                default:
                    ctx.Event.User.SendMessage("Not supported.");
                    return CommandResult.InvalidArguments;
            }
        }

        private CommandResult GetProperty(CommandContext ctx)
        {
            if (!ctx.Args.TryDequeue(out var category))
            {
                Logger.LogWarning($"Could not retrieve `{nameof(category)}`. {ctx.GetArgumentsForLog()}");
                return CommandResult.MissingArguments;
            }

            switch (category)
            {
                case "unit":
                {
                    if (!ctx.Args.TryDequeue(out var property))
                    {
                        Logger.LogWarning($"Could not retrieve `{nameof(property)}`. {ctx.GetArgumentsForLog()}");
                        return CommandResult.MissingArguments;
                    }

                    try
                    {
                        var component = ctx.EntityManager.GetComponentData<UnitStats>(ctx.CharacterEntity);

                        switch (property)
                        {
                            case "p crit rate":
                            case "p crit chance":
                            {
                                var value = component.PhysicalCriticalStrikeChance.Value;
                                ctx.EventUser.SendMessage(
                                    $"The value of \"{nameof(component.PhysicalCriticalStrikeChance)}\" is {value}");
                                break;
                            }

                            case "regen":
                            {
                                var value = component.PassiveHealthRegen.Value;
                                ctx.EventUser.SendMessage(
                                    $"The value of \"{nameof(component.PassiveHealthRegen)}\" is {value}");
                                break;
                            }

                            case "res power":
                            case "res pwr":
                            case "res pow":
                            case "resource power":
                            case "resource pow":
                            case "resource pwr":
                            {
                                var value = component.ResourcePower.Value;
                                ctx.EventUser.SendMessage(
                                    $"The value of \"{nameof(component.PassiveHealthRegen)}\" is {value}");
                                break;
                            }

                            case "res yield":
                            case "res yld":
                            case "resource yield":
                            case "resource yld":
                            {
                                var value = component.ResourceYieldModifier.Value;
                                ctx.EventUser.SendMessage(
                                    $"The value of \"{nameof(component.PassiveHealthRegen)}\" is {value}");
                                break;
                            }

                            default:
                            {
                                if (typeof(UnitStats).GetField(property) is {IsPublic: true,} memberInfo)
                                {
                                    if (memberInfo.FieldType == typeof(ModifiableBool))
                                    {
                                        var me = UnitStatsCache.Get2<ModifiableBool>(memberInfo.Name);
                                        var mutable = me.GetValue(component);
                                        ctx.EventUser.SendMessage(
                                            $"The value of \"{memberInfo.Name}\" is {mutable.Value}");
                                    }
                                    else if (memberInfo.FieldType == typeof(ModifiableInt))
                                    {
                                        var me = UnitStatsCache.Get2<ModifiableInt>(memberInfo.Name);
                                        var mutable = me.GetValue(component);
                                        ctx.EventUser.SendMessage(
                                            $"The value of \"{memberInfo.Name}\" is {mutable.Value}");
                                    }
                                    else if (memberInfo.FieldType == typeof(ModifiableFloat))
                                    {
                                        var me = UnitStatsCache.Get2<ModifiableFloat>(memberInfo.Name);
                                        var mutable = me.GetValue(component);
                                        ctx.EventUser.SendMessage(
                                            $"The value of \"{memberInfo.Name}\" is {mutable.Value}");
                                    }
                                }
                                else
                                    ctx.EventUser.SendMessage(
                                        $"The option \"{property}\" is unsupported at this time.");

                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ctx.EventUser.SendMessage("An error occurred trying to access the requested data");
                        Logger.LogError(e);
                    }

                    break;
                }
                default:
                    return CommandResult.InvalidArguments;
            }

            return CommandResult.Success;
        }

        private CommandResult SetProperty(CommandContext ctx)
        {
            if (!ctx.Args.TryDequeue(out var category))
            {
                Logger.LogWarning($"Could not retrieve `{nameof(category)}`. {ctx.GetArgumentsForLog()}");
                return CommandResult.MissingArguments;
            }

            var message = string.Empty;
            switch (category)
            {
                case "unit":
                {
                    if (!ctx.Args.TryDequeue(out var property))
                    {
                        Logger.LogWarning($"Could not retrieve `{nameof(property)}`. {ctx.GetArgumentsForLog()}");
                        return CommandResult.MissingArguments;
                    }

                    try
                    {
                        var component = ctx.EntityManager.GetComponentData<UnitStats>(ctx.CharacterEntity);

                        switch (property)
                        {
                            case nameof(component.PhysicalCriticalStrikeChance):
                            case "p crit rate":
                            case "p crit chance":
                            {
                                if (!ctx.Args.TryDequeueFloat(out var stringValue, out var value))
                                {
                                    Logger.LogWarning(
                                        $"Could not parse {stringValue} as a float `{nameof(value)}`. {ctx.GetArgumentsForLog()}");
                                    return CommandResult.InvalidArguments;
                                }

                                component.PhysicalCriticalStrikeChance =
                                    ModifiableFloat.Create(ctx.CharacterEntity, ctx.EntityManager, value);
                                message = $"The value of \"{property}\" has been set to {value}";
                                break;
                            }

                            case nameof(component.PassiveHealthRegen):
                            case "regen":
                            {
                                if (!ctx.Args.TryDequeueFloat(out var stringValue, out var value))
                                {
                                    Logger.LogWarning(
                                        $"Could not parse {stringValue} as a float `{nameof(value)}`. {ctx.GetArgumentsForLog()}");
                                    return CommandResult.InvalidArguments;
                                }


                                component.PassiveHealthRegen =
                                    ModifiableFloat.Create(ctx.CharacterEntity, ctx.EntityManager, value);
                                message = $"The value of \"{property}\" has been set to {value}";
                                break;
                            }

                            case nameof(component.ResourcePower):
                            case "res power":
                            case "res pwr":
                            case "res pow":
                            case "resource power":
                            case "resource pow":
                            case "resource pwr":
                            {
                                if (!ctx.Args.TryDequeueFloat(out var stringValue, out var value))
                                {
                                    Logger.LogWarning(
                                        $"Could not parse {stringValue} as a float `{nameof(value)}`. {ctx.GetArgumentsForLog()}");
                                    return CommandResult.InvalidArguments;
                                }

                                component.ResourcePower =
                                    ModifiableFloat.Create(ctx.CharacterEntity, ctx.EntityManager, value);
                                message = $"The value of \"{property}\" has been set to {value}";
                                break;
                            }

                            case nameof(component.ResourceYieldModifier):
                            case "res yield":
                            case "res yld":
                            case "resource yield":
                            case "resource yld":
                            {
                                if (!ctx.Args.TryDequeueFloat(out var stringValue, out var value))
                                {
                                    Logger.LogWarning(
                                        $"Could not parse {stringValue} as a float. {ctx.GetArgumentsForLog()}");
                                    return CommandResult.InvalidArguments;
                                }

                                component.ResourceYieldModifier =
                                    ModifiableFloat.Create(ctx.CharacterEntity, ctx.EntityManager, value);
                                message = $"The value of \"{property}\" has been set to {value}";
                                break;
                            }

                            default:
                            {
                                if (typeof(UnitStats).GetField(property) is {IsPublic: true,} memberInfo)
                                {
                                    if (memberInfo.FieldType == typeof(ModifiableBool))
                                    {
                                        if (ctx.Args.TryDequeueBool(out var stringValue, out var value))
                                        {
                                            var me = UnitStatsCache.Get2<ModifiableBool>(memberInfo.Name);
                                            me.SetValue(component,
                                                ModifiableBool.Create(ctx.CharacterEntity, ctx.EntityManager, value));
                                            message = $"The value of \"{property}\" has been set to {value}";
                                        }
                                        else
                                        {
                                            Logger.LogWarning(
                                                $"Could not parse {stringValue} as a bool. {ctx.GetArgumentsForLog()}");
                                            return CommandResult.InvalidArguments;
                                        }
                                    }
                                    else if (memberInfo.FieldType == typeof(ModifiableInt))
                                    {
                                        if (ctx.Args.TryDequeueInt(out var stringValue, out var value))
                                        {
                                            var me = UnitStatsCache.Get2<ModifiableInt>(memberInfo.Name);
                                            me.SetValue(component,
                                                ModifiableInt.Create(ctx.CharacterEntity, ctx.EntityManager, value));
                                            message = $"The value of \"{property}\" has been set to {value}";
                                        }
                                        else
                                        {
                                            Logger.LogWarning(
                                                $"Could not parse {stringValue} as an int. {ctx.GetArgumentsForLog()}");
                                            return CommandResult.InvalidArguments;
                                        }
                                    }
                                    else if (memberInfo.FieldType == typeof(ModifiableFloat))
                                    {
                                        if (ctx.Args.TryDequeueInt(out var stringValue, out var value))
                                        {
                                            var me = UnitStatsCache.Get2<ModifiableFloat>(memberInfo.Name);
                                            me.SetValue(component,
                                                ModifiableFloat.Create(ctx.CharacterEntity, ctx.EntityManager, value));
                                            message = $"The value of \"{property}\" has been set to {value}";
                                        }
                                        else
                                        {
                                            Logger.LogWarning(
                                                $"Could not parse {stringValue} as a float. {ctx.GetArgumentsForLog()}");
                                            return CommandResult.InvalidArguments;
                                        }
                                    }
                                }

                                break;
                            }
                        }

                        ctx.EntityManager.SetComponentData(ctx.CharacterEntity, component);
                    }
                    catch (Exception e)
                    {
                        ctx.EventUser.SendMessage("An error occurred trying to access the requested data");
                        Logger.LogError(e);
                    }

                    break;
                }
                default:
                    return CommandResult.InvalidArguments;
            }

            ctx.EventUser.SendMessage(message);
            return CommandResult.Success;
        }
    }
}