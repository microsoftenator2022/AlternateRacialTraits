using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features;

internal static class HeritageFeatures
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
                var (skilledFeature, heritage) = bps;

                if (heritage is null)
                    throw new NullReferenceException();

                var displayNameKey = $"{baseDisplayName.Key}.{skilledFeature.name}";

                LocalizedStrings.DefaultStringEntries.Add(
                        displayNameKey,
                        $"{heritage.Name} - Skilled");

                skilledFeature.m_DisplayName = new LocalizedString { m_Key = displayNameKey };

                skilledFeature.HideInUI = true;

                foreach (var addStatBonus in heritage.GetComponents<AddStatBonus>().Where(asb => StatTypeHelper.Skills.Contains(asb.Stat)))
                {
                    heritage.RemoveComponent(addStatBonus);
                    skilledFeature.AddComponent(addStatBonus);
                }

                var addFacts = heritage.EnsureComponent<AddFacts>();
                addFacts.m_Facts ??= [];
                addFacts.m_Facts = addFacts.m_Facts.Append(skilledFeature.ToReference<BlueprintUnitFactReference>());

                return (skilledFeature, heritage);
            });

        _ = blueprints.Map(pair => pair.heritage)
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

            var displayNameKey = $"{baseDisplayName.Key}.{feature.name}";

            LocalizedStrings.DefaultStringEntries.Add(
                    displayNameKey,
                    $"{heritageFeature.Name} - Spell-like Ability");

            feature.m_DisplayName = new LocalizedString { m_Key = displayNameKey };

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

    internal static IInitContext<BlueprintComponent[]> SkilledPrerequisiteComponents(IInitContext<BlueprintFeature[]> skilledFeatures)
    {
        return skilledFeatures
            .Map(features =>
            {
                var facts = features.Select(f => f.ToReference<BlueprintUnitFactReference>());

                return facts
                    .Select(featureRef => new RemoveFeatureOnApply { m_Feature = featureRef })
                    .Append<BlueprintComponent>(new PrerequisiteFeaturesFromList
                    {
                        Amount = 1,
                        Group = Prerequisite.GroupType.All,
                        m_Features = features.Select(f => f.ToReference()).ToArray()
                    })
                    .ToArray();
            });
    }

    internal static IInitContext<BlueprintComponent[]> SLAPrerequisiteComponents(IInitContext<(BlueprintFeature feature, BlueprintAbility[] facts)[]> slaFeatures) =>
        slaFeatures.Map(heritageFeatures =>
        {
            var facts = heritageFeatures
                .SelectMany(pair => pair.facts.Append<BlueprintUnitFact>(pair.feature))
                .Select(f => f.ToMicroBlueprint());

            return facts
                .Select(f => new RemoveFeatureOnApply { m_Feature = f.ToReference() })
                .Append<BlueprintComponent>(
                    new PrerequisiteFeaturesFromList
                    {
                        Amount = 1,
                        Group = Prerequisite.GroupType.All,
                        m_Features = heritageFeatures.Select(pair => pair.feature.ToReference()).ToArray()
                    })
                .ToArray();
        });

}
