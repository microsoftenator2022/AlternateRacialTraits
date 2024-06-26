using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Deferred;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human;

internal static class PracticedHunter
{
    [LocalizedString]
    public static readonly string DisplayName = "Practiced Hunter";
    [LocalizedString]
    public static readonly string Description =
        "Members of some human cultures train from youth to find and follow the trails of vital game and " +
        "at the same time hide the evidence of their own passage. These humans gain a +2 racial " +
        $"{new Link(Page.Bonus, "bonus")} on {new Link(Page.Stealth, "Stealth")} and " +
        $"{new Link(Page.Lore_Nature, "Lore (Nature)")} " +
        $"{new Link(Page.Check, "checks")}, and Stealth and Lore (Nature) are always class " +
        $"skills for them. This racial {new Link(Page.Trait, "trait")} replaces Skilled.";

    internal static IDeferred<BlueprintFeature> Create() =>
        Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.PracticedHunter)
            .Map((BlueprintFeature blueprint) =>
            {
                blueprint.m_DisplayName = LocalizedStrings.Features_Human_PracticedHunter_DisplayName;
                blueprint.m_Description = LocalizedStrings.Features_Human_PracticedHunter_Description;

                blueprint.Groups = [FeatureGroup.Racial];

                blueprint.SetIcon("3482eb9c0d448524ab950213b3866301", 21300000);

                _ = blueprint.AddAddStatBonus(c =>
                {
                    c.Stat = StatType.SkillStealth;
                    c.Value = 2;
                    c.Descriptor = ModifierDescriptor.Racial;
                });

                _ = blueprint.AddAddStatBonus(c =>
                {
                    c.Stat = StatType.SkillLoreNature;
                    c.Value = 2;
                    c.Descriptor = ModifierDescriptor.Racial;
                });

                _ = blueprint.AddComponent<AddClassSkill>(c =>
                {
                    c.Skill = StatType.SkillStealth;
                });

                _ = blueprint.AddComponent<AddClassSkill>(c =>
                {
                    c.Skill = StatType.SkillLoreNature;
                });

                _ = blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, hideInUI: false, removeOnApply: true);

                return blueprint;
            })
            .AddBlueprintDeferred(GeneratedGuid.PracticedHunter);
            //.AddOnTrigger(GeneratedGuid.PracticedHunter, Triggers.BlueprintsCache_Init);
}
