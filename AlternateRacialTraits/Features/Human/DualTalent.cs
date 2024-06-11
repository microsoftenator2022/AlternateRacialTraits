﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static partial class DualTalent
    {
        [AllowedOn(typeof(BlueprintFeature))]
        internal class PrerequisiteNoRaceAttributeBonus : Prerequisite
        {
            public StatType Stat;

            public override bool CheckInternal(
                FeatureSelectionState selectionState,
                UnitDescriptor unit,
                LevelUpState state)
            {
                var statValue = unit.Stats.Attributes.First(s => s.Type == Stat);

                return !statValue.Modifiers.Any(m =>
                    m.ModDescriptor == ModifierDescriptor.Racial &&
                    m.Source.Blueprint == unit.Progression.Race &&
                    m.ModValue > 0);
            }

            public override string GetUITextInternal(UnitDescriptor unit) =>
                $"{UIStrings.Instance.Tooltips.NoFeature}: {LocalizedTexts.Instance.Stats.GetText(Stat)}";
        }

        [LocalizedString]
        public static readonly string DisplayName = "Dual Talent";

        [LocalizedString]
        public static readonly string Description =
            "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two " +
            $"{new Link(Page.Ability_Scores, "ability scores")} and gain a +2 racial " +
            $"{new Link(Page.Bonus, "bonus")} in each of those scores. This racial trait replaces the +2 " +
            "bonus to any one ability score, the bonus feat trait and the Skilled trait.";

        internal static IInitContext<BlueprintFeature> Create()
        {
            var stats = (new[]
            {
                StatType.Strength,
                StatType.Dexterity,
                StatType.Constitution,
                StatType.Intelligence,
                StatType.Wisdom,
                StatType.Charisma
            })
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
                            LocalizedStrings.DefaultStringEntries.Add(displayNameKey, $"{DisplayName} - {statName}");

                            blueprint.m_DisplayName = new LocalizedString { m_Key = displayNameKey };
                            blueprint.m_Description = Localized.Description;

                            blueprint.m_Icon = (UnityEngine.Sprite?)LocalizedTexts.Instance.Stats.GetIcon(stat);

                            blueprint.Groups = [FeatureGroup.Racial];

                            blueprint.AddAddStatBonus(c =>
                            {
                                c.Stat = stat;
                                c.Value = 2;
                                c.Descriptor = ModifierDescriptor.Racial;
                            });

                            blueprint.AddComponent<PrerequisiteNoRaceAttributeBonus>(c => c.Stat = stat);

                            return blueprint;
                        })
                        .RegisterBlueprint(guid, Triggers.BlueprintsCache_Init);
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
                    
                    selection.AddComponent<OverrideSelectionPriority>(c =>
                        c.Priority = Kingmaker.UI.MVVM._VM.CharGen.Phases.CharGenPhaseBaseVM
                            .ChargenPhasePriority.AbilityScores);

                    return selection;
                })
                .RegisterBlueprint(GeneratedGuid.DualTalentSelection, Triggers.BlueprintsCache_Init);

            return InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.DualTalent)
                .Combine(selection)
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
                .RegisterBlueprint(GeneratedGuid.DualTalent, Triggers.BlueprintsCache_Init);
        }
    }
}
