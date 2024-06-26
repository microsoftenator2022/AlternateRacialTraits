using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features;

internal static partial class HeritageFeatures
{
    internal static IInitContextBlueprint<BlueprintFeature> CreateSkillFeature(
        IMicroBlueprint<BlueprintFeature> heritageFeature, string name,
        LocalizedString baseDisplayName)
    {
        var guid = GeneratedGuid.Get(name);

        var blueprints = InitContext.NewBlueprint<BlueprintFeature>(guid)
            .Combine(InitContext.GetBlueprint(heritageFeature))
            .Map(bps =>
            {
                var (skilledFeature, heritageFeature) = bps;

                if (heritageFeature is null)
                    throw new NullReferenceException();

                if (!string.IsNullOrEmpty(heritageFeature.m_DisplayName?.Key))
                {
                    var displayNameKey = $"{baseDisplayName.Key}.{skilledFeature.name}";

                    LocalizedStrings.DefaultStringEntries.Add(
                        displayNameKey,
                        $"{heritageFeature.Name} - Skilled");

                    skilledFeature.m_DisplayName = new LocalizedString { m_Key = displayNameKey };
                }

                skilledFeature.HideInUI = true;

                foreach (var addStatBonus in heritageFeature.GetComponents<AddStatBonus>().Where(asb => StatTypeHelper.Skills.Contains(asb.Stat)))
                {
                    heritageFeature.RemoveComponent(addStatBonus);
                    skilledFeature.AddComponent(addStatBonus);
                }

                var addFacts = heritageFeature.EnsureComponent<AddFacts>();
                addFacts.m_Facts ??= [];
                addFacts.m_Facts = addFacts.m_Facts.Append(skilledFeature.ToReference<BlueprintUnitFactReference>());

                return (skilledFeature, heritageFeature);
            });

        _ = blueprints.Map(pair => pair.heritageFeature)
            .OnDemand(heritageFeature.BlueprintGuid);

        return blueprints.Map(pair => pair.skilledFeature)
            .AddBlueprintDeferred(guid);
    }

    internal static IInitContext<Option<IInitContext<(BlueprintFeature, BlueprintAbility[])>>> CreateHeritageSLAFeature(
        IMicroBlueprint<BlueprintFeature> heritageFeature, string name, LocalizedString baseDisplayName)
    {
        var guid = GeneratedGuid.Get(name);
        
        var maybeFeatures = InitContext.NewBlueprint<BlueprintFeature>(guid)
            .Combine(InitContext.GetBlueprint(heritageFeature))
            .Map(bps =>
            {
                var (feature, heritageFeature) = bps;

                if (heritageFeature is null)
                    throw new NullReferenceException();

                if (heritageFeature.GetComponent<AddFacts>() is not { } af)
                    return Option<(BlueprintFeature feature, BlueprintAbility[] facts, BlueprintFeature heritageFeature)>.None;

                if (!string.IsNullOrEmpty(heritageFeature.m_DisplayName?.Key))
                {
                    var displayNameKey = $"{baseDisplayName.Key}.{feature.name}";

                    LocalizedStrings.DefaultStringEntries.Add(
                        displayNameKey,
                        $"{heritageFeature.Name} - Spell-like Ability");

                    feature.m_DisplayName = new LocalizedString { m_Key = displayNameKey };
                }

                feature.HideInUI = true;

                var facts = af.m_Facts.Select(f => f.Get()).OfType<BlueprintAbility>().ToArray();

                af.m_Facts = af.m_Facts.Append(feature.ToReference<BlueprintUnitFactReference>());

                return Option.Some((feature, facts, heritageFeature));
            });
        
        return
            maybeFeatures.MapOption(blueprints =>
            {
                _ = new InitContext<BlueprintFeature>(() => blueprints.heritageFeature)
                    .OnDemand(blueprints.heritageFeature.AssetGuid);

                var feature = new InitContext<BlueprintFeature>(() => blueprints.feature)
                        .AddBlueprintDeferred(blueprints.feature.AssetGuid);

                var facts = new InitContext<BlueprintAbility[]>(() => blueprints.facts);

                return (feature.Combine(facts));
            });
    }

    [LocalizedString]
    internal const string SkilledPrerequisiteDisplayName = "Skilled";

    internal static IInitContext<BlueprintComponent[]> SkilledPrerequisiteComponents(
        IInitContext<BlueprintFeature[]> skilledFeatures)
    {
        return skilledFeatures
            .Map(features =>
            {
                var facts = features.Select(f => f.ToReference<BlueprintUnitFactReference>());

                return facts
                    .Select(featureRef => new RemoveFeatureOnApply { m_Feature = featureRef })
                    .Append<BlueprintComponent>(new HeritageFeaturePrerequisite
                    {
                        DisplayName = Localized.SkilledPrerequisiteDisplayName,
                        Features = features.Select(f => f.ToReference()).ToArray(),
                        Group = Prerequisite.GroupType.All
                    })
                    .ToArray();
            });
    }

    [LocalizedString]
    internal const string SLAPrerequisiteDisplayName = "Spell-like Ability";

    internal static IInitContext<BlueprintComponent[]> SLAPrerequisiteComponents(
        IInitContext<(BlueprintFeature feature, BlueprintAbility[] facts)[]> slaFeatures) =>
        slaFeatures.Map(heritageFeatures =>
        {
            var facts = heritageFeatures
                .SelectMany(pair => pair.facts.Append<BlueprintUnitFact>(pair.feature))
                .Select(f => f.ToMicroBlueprint());

            return facts
                .Select(f => new RemoveFeatureOnApply { m_Feature = f.ToReference() })
                .Append<BlueprintComponent>(
                    new HeritageFeaturePrerequisite
                    {
                        DisplayName = Localized.SLAPrerequisiteDisplayName,
                        Features = heritageFeatures.Select(pair => pair.feature.ToReference()).ToArray(),
                        Group = Prerequisite.GroupType.All
                    })
                .ToArray();
        });
}

class HeritageFeaturePrerequisite : Prerequisite
{
    public BlueprintFeatureReference[] Features = [];

    public LocalizedString DisplayName = Default.LocalizedString;

    public override bool CheckInternal(FeatureSelectionState? selectionState, UnitDescriptor unit, LevelUpState? state)
    {
        foreach (var featureReference in this.Features)
        {
            var feature = featureReference.Get();
            if (unit.HasFact(feature) && (selectionState is null || !selectionState.IsSelectedInChildren(feature)))
            {
                return true;
            }
        }

        return false;
    }

    public override string GetUITextInternal(UnitDescriptor unit) => this.DisplayName;
}
