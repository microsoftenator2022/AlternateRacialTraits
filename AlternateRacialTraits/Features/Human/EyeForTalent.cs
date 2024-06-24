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
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UI.MVVM._VM.CharGen.Phases;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human;

internal static partial class EyeForTalent
{
    [LocalizedString]
    internal const string DisplayName = "Eye for Talent";
    
    [LocalizedString]
    internal static readonly string Description =
        "Humans have great intuition for hidden potential. They gain a +2 racial " +
        $"{new Link(Page.Bonus, "bonus")} on {new Link(Page.Perception, "Perception")} checks. " +
        "In addition, when they acquire an animal companion, bonded mount, cohort, or familiar, that creature " +
        $"gains a +2 {new Link(Page.Bonus, "bonus")} to one ability score of the character’s choice. " +
        $"This {new Link(Page.Trait, "racial trait")} replaces the bonus " +
        $"{new Link(Page.Feat, "feat")} trait.";

    public static IInitContextBlueprint<BlueprintFeature> Create()
    {
        var bonusSelection =
            InitContext.NewBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.Get("EyeForTalentCompanionBonus"))
                .Map(selection =>
                {
                    selection.m_DisplayName = Localized.DisplayName;
                    selection.m_Description = Localized.Description;

                    selection.ParameterType = FeatureParameterType.Skill;

                    _ = selection.AddComponent<ParamStats>(c => c.Stats = StatTypeHelper.Attributes);

                    _ = selection.AddAddParametrizedStatBonus(c => c.Value = 2);

                    _ = selection.AddComponent<SelectionPriority>(c => c.PhasePriority = CharGenPhaseBaseVM.ChargenPhasePriority.AbilityScores);

                    return selection;
                })
                .AddBlueprintDeferred(GeneratedGuid.EyeForTalentCompanionBonus);

        var companionProgression = 
            InitContext.NewBlueprint<BlueprintProgression>(GeneratedGuid.Get("EyeForTalentCompanionProgression"))
                .Combine(bonusSelection)
                .Map(bps =>
                {
                    var (progression, selection) = bps;

                    progression.m_DisplayName = Localized.DisplayName;

                    progression.AddFeatures(1, [selection]);

                    return progression;
                })
                .AddBlueprintDeferred(GeneratedGuid.EyeForTalentCompanionProgression);

        var feature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("EyeForTalentFeature"))
            .Combine(companionProgression)
            .Map(bps =>
            {
                var (feature, companionProgression) = bps;

                feature.m_DisplayName = Localized.DisplayName;
                feature.m_Description = Localized.Description;

                _ = feature.AddAddStatBonus(c =>
                {
                    c.Stat = StatType.SkillPerception;
                    c.Value = 2;
                    c.Descriptor = ModifierDescriptor.Racial;
                });

                _ = feature.AddAddFeatureToPet(c =>
                {
                    c.m_PetType = PetType.AnimalCompanion;
                    c.m_Feature = companionProgression.ToReference<BlueprintFeatureReference>();
                });
                
                return feature;
            })
            .AddBlueprintDeferred(GeneratedGuid.EyeForTalentFeature);

        return feature;
    }
}
