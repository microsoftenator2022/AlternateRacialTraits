using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar;

internal static partial class DeathlessSpirit
{
    [LocalizedString]
    internal const string DisplayName = "Deathless Spirit";

    [LocalizedString]
    internal static readonly string Description =
        "Particularly strong-willed aasimars possess celestial spirits capable of resisting the powers of death. " +
        $"They gain {new Link(Page.Energy_Resistance, "resistance")} 5 against negative energy damage. " +
        $"They do not lose {new Link(Page.HP, "hit points")} when they gain a " +
        $"negative level, and they gain a +2 racial {new Link(Page.Bonus, "bonus")} on " +
        $"{new Link(Page.Saving_Throw, "saving throws")} against death effects, energy drain, negative energy, " +
        $"and {new Link(Page.Spells, "spells")} or spell-like abilities of the " +
        $"{new Link(Page.Necromancy, "necromancy")} school. " +
        $"This racial {new Link(Page.Trait, "trait")} replaces celestial resistance.";

    [HarmonyPatch]
    internal class DeathlessSpiritComponent : BlueprintComponent
    {
        static bool HasDeathlessSpirit(UnitEntityData unit) =>
            unit.Facts.Contains(f => f.GetComponent<DeathlessSpiritComponent>() is not null);

        [HarmonyPatch(typeof(NegativeLevelComponent), nameof(NegativeLevelComponent.Apply))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var index = instructions.FindIndex(
                ci => ci.opcode == OpCodes.Ldfld && 
                (FieldInfo)ci.operand == AccessTools.Field(typeof(CharacterStats), nameof(CharacterStats.HitPoints)));

            if (index < 0)
                throw new Exception("Could not find target instruction");

            // iList[index - 2] calls get_Owner

            var iList = instructions.ToList();

            var ifFalse = ilGen.DefineLabel();

            iList[index - 1].labels.Add(ifFalse);

            iList.InsertRange(
                index - 1,
                [
                    new(OpCodes.Dup),
                    CodeInstruction.Call((UnitEntityData unit) => HasDeathlessSpirit(unit)),
                    new(OpCodes.Brfalse_S, ifFalse),
                    new(OpCodes.Pop),
                    new(OpCodes.Pop),
                    new(OpCodes.Ret)
                ]);

            return iList;
        }
    }

    internal static IInitContextBlueprint<BlueprintFeature> Create()
    {
        var guid = new BlueprintGuid(Guid.NewGuid());

        return
            InitContext.NewBlueprint<BlueprintFeature>(guid, nameof(DeathlessSpirit))
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = Localized.DisplayName;
                    blueprint.m_Description = Localized.Description;

                    blueprint.SetIcon("de12a23036a3a954793c106c9062b9f9", 21300000);

                    _ = blueprint.AddAddDamageResistanceEnergy(c =>
                    {
                        c.Type = DamageEnergyType.NegativeEnergy;
                        c.Value = 5;
                    });

                    _ = blueprint.AddSavingThrowBonusAgainstDescriptor(c =>
                    {
                        c.Value = 2;
                        c.ModifierDescriptor = ModifierDescriptor.Racial;
                        c.SpellDescriptor = SpellDescriptor.Death | SpellDescriptor.NegativeLevel | SpellDescriptor.ChannelNegativeHarm;
                    });

                    _ = blueprint.AddSavingThrowBonusAgainstSchool(c =>
                    {
                        c.Value = 2;
                        c.ModifierDescriptor = ModifierDescriptor.Racial;
                        c.School = SpellSchool.Necromancy;
                    });

                    _ = blueprint.AddComponent<DeathlessSpiritComponent>();

                    _ = blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.CelestialResistance, removeOnApply: true);

                    return blueprint;
                })
                .AddBlueprintDeferred(guid);
    }
}
