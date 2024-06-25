using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;

using MicroWrath;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human;

internal static partial class DualTalent
{
    [LocalizedString]
    public static readonly string DisplayName = "Dual Talent";

    [LocalizedString]
    public static readonly string Description =
        "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two " +
        $"{new Link(Page.Ability_Scores, "ability scores")} and gain a +2 racial " +
        $"{new Link(Page.Bonus, "bonus")} in each of those scores. " +
        $"This racial {new Link(Page.Trait, "trait")} replaces the +2 " +
        $"bonus to any one ability score, the bonus {new Link(Page.Feat, "feat")} trait " +
        "and the Skilled trait.";

    internal static IInitContext<BlueprintFeature> Create()
    {
#region Obsolete
        var stats = StatTypeHelper.Attributes
        .Select(stat =>
        {
            var bpName = $"DualTalent{Enum.GetName(typeof(StatType), stat)}";
            var guid = GeneratedGuid.Get(bpName);

            return (guid.Guid, bpName, stat);
        });

        var dualTalentFeatures = stats
            .Select(items =>
            {
                var (guid, name, stat) = items;

                return InitContext.NewBlueprint<BlueprintFeature>(guid, name)
                    .Map(blueprint =>
                    {
                        var statName = LocalizedTexts.Instance.Stats.GetText(stat);

                        var displayNameKey = $"{Localized.DisplayName.Key}.{blueprint.name}";
                        LocalizedStrings.DefaultStringEntries.Add(displayNameKey, $"{DisplayName} - {statName} (Obsolete)");

                        blueprint.m_DisplayName = new LocalizedString { m_Key = displayNameKey };
                        blueprint.m_Description = Localized.Description;

                        blueprint.m_Icon = (UnityEngine.Sprite?)LocalizedTexts.Instance.Stats.GetIcon(stat);

                        blueprint.Groups = [FeatureGroup.Racial];

                        _ = blueprint.AddAddStatBonus(c =>
                        {
                            c.Stat = stat;
                            c.Value = 2;
                            c.Descriptor = ModifierDescriptor.Racial;
                        });

                        _ = blueprint.AddComponent<PrerequisiteNoRaceStatBonus>(c => c.Stat = stat);

                        return blueprint;
                    })
                    .AddBlueprintDeferred(guid);
                    //.AddOnTrigger(guid, Triggers.BlueprintsCache_Init);
            });

        var selection = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.DualTalentSelection, $"{nameof(DualTalent)}Selection")
            .Combine(dualTalentFeatures.Collect())
            .Map(bps =>
            {
                var (selection, features) = bps;

                selection.m_DisplayName = LocalizedStrings.Features_Human_DualTalent_DisplayName;
                selection.m_Description = LocalizedStrings.Features_Human_DualTalent_Description;

                selection.SetIcon("0465d85ec271d694f93089def152b820", 21300000);

                selection.AddFeatures(features.Select(bp => bp.ToMicroBlueprint()));

                selection.Groups = [FeatureGroup.Racial];

                _ = selection.AddComponent<SelectionPriority>(c =>
                    c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.CharGenPhaseBaseVM
                        .ChargenPhasePriority.AbilityScores);

                return selection;
            })
            .AddBlueprintDeferred(GeneratedGuid.DualTalentSelection);
            //.AddOnTrigger(GeneratedGuid.DualTalentSelection, Triggers.BlueprintsCache_Init);

#endregion
        var bonusFeature =
            InitContext.NewBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.Get("DualTalentBonus"))
                .Map(selection =>
                {
                    selection.m_DisplayName = Localized.DisplayName;
                    selection.m_Description = Localized.Description;

                    selection.SetIcon("0465d85ec271d694f93089def152b820", 21300000);

                    selection.Groups = [FeatureGroup.Racial];

                    selection.ParameterType = FeatureParameterType.Skill;

                    _ = selection.AddComponent<ParamStats>(c => c.Stats = StatTypeHelper.Attributes);

                    _ = selection.AddAddParametrizedStatBonus(c =>
                    {
                        c.Value = 2;
                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    _ = selection.AddComponent<SelectionPriority>(c =>
                    {
                        c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.CharGenPhaseBaseVM
                            .ChargenPhasePriority.AbilityScores;

                        //c.ActionPriority = Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpActionPriority.RaceStat;
                    });

                    _ = selection.AddComponent<PrerequisiteNoRaceStatBonus>(c => c.Parametrized = true);

                    return selection;
                })
                .AddBlueprintDeferred(GeneratedGuid.DualTalentBonus);
                //.AddOnTrigger(GeneratedGuid.DualTalentBonus, Triggers.BlueprintsCache_Init);

        return InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.DualTalent)
            .Combine(bonusFeature)
            .Map(bps =>
            {
                var (feature, selection) = bps;

                feature.m_DisplayName = Localized.DisplayName;
                feature.m_Description = Localized.Description;

                feature.m_Icon = selection.Icon;

                //feature.AddAddFacts(c => c.m_Facts = new[] { selection.ToReference<BlueprintUnitFactReference>() });

                var selectionRef = selection.ToReference<BlueprintFeatureBaseReference>();

                feature.AddComponent(new UnitFactActivateEvent(e =>
                {
                    Util.AddLevelUpSelection(e.Owner, [selectionRef], e.Owner.Progression.Race);
                }));

                feature.Groups = [FeatureGroup.Racial];

                return feature;
            })
            .AddBlueprintDeferred(GeneratedGuid.DualTalent);
            //.AddOnTrigger(GeneratedGuid.DualTalent, Triggers.BlueprintsCache_Init);
    }
}
