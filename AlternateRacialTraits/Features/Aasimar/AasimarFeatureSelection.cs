using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Localization;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

using UniRx;
#if DEBUG
namespace AlternateRacialTraits.Features.Aasimar
{
    internal static partial class AasimarFeatureSelection
    {
        [LocalizedString]
        internal const string DisplayName = "Alternate Racial Traits";

        [LocalizedString]
        internal const string Description = "The following alternate traits are available";

        static IInitContextBlueprint<BlueprintFeature> CreateHeritageSkillFeature(
            IMicroBlueprint<BlueprintFeature> heritageFeature, string name)
        {
            var guid = GeneratedGuid.Get(name);

            var blueprints = InitContext.NewBlueprint<BlueprintFeature>(guid)
                .Combine(InitContext.GetBlueprint(heritageFeature))
                .Map(bps =>
                {
                    var (skilledFeature, heritage) = bps;

                    if (heritage is null)
                        throw new NullReferenceException();

                    var displayNameKey = $"{Localized.DisplayName.Key}.{skilledFeature.name}";

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

            blueprints.Map(pair => pair.heritage)
                .OnDemand(heritageFeature.BlueprintGuid);

            return blueprints.Map(pair => pair.skilledFeature)
                .AddBlueprintDeferred(guid);
        }

        internal static IInitContext<IEnumerable<BlueprintFeature>> HeritageFeatures =>
            InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeatureSelection.AasimarHeritageSelection)
                .Bind(selection =>
                {
                    var features = selection.AllFeatures;

                    return features
                        .Select(f => InitContext.GetBlueprint(f.ToMicroBlueprint()))
                        .Collect();
                })
                .Map(features => features.NotNull());

        internal static readonly Lazy<IInitContext<BlueprintFeature[]>> SkilledFeatures = new(() =>
        {
            return HeritageFeatures
                .Bind(features => features
                    .Select(f => CreateHeritageSkillFeature(f.ToMicroBlueprint(), $"{f.Name}Skilled"))
                    .Collect())
                .Map(Enumerable.ToArray);
        });

        [HarmonyPatch]
        static class UIUtilityUnit_Patch
        {
            [HarmonyPatch(typeof(UIUtilityUnit), nameof(UIUtilityUnit.ParseFeatureOnImmediatelyAbilities))]
            [HarmonyPostfix]
            static void ParseFeatureOnImmediatelyAbilities(BlueprintFeatureBase feature, List<IUIDataProvider> __result)
            {
                if (feature.AssetGuid != GeneratedGuid.Plumekith__Garuda_Blooded_Ability.Guid)
                    return;

                MicroLogger.Debug(() => $"{nameof(GeneratedGuid.Plumekith__Garuda_Blooded_Ability)} {feature}: {__result.Count}");
            }
        }

        static IInitContext<Option<IInitContext<(BlueprintFeature, BlueprintAbility[])>>> CreateHeritageSLAFeature(
            IMicroBlueprint<BlueprintFeature> heritageFeature,
            string name)
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

                    var displayNameKey = $"{Localized.DisplayName.Key}.{feature.name}";

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
                    new InitContext<BlueprintFeature>(() => blueprints.heritageFeature)
                        .OnDemand(blueprints.heritageFeature.AssetGuid);

                    var feature = new InitContext<BlueprintFeature>(() => blueprints.feature)
                            .AddBlueprintDeferred(blueprints.feature.AssetGuid);

                    var facts = new InitContext<BlueprintAbility[]>(() => blueprints.facts);

                    return (feature.Combine(facts));
                });
        }

        internal static readonly Lazy<IInitContext<(BlueprintFeature feature, BlueprintAbility[] facts)[]>> SLAFeatures = new(() =>
        {
            return HeritageFeatures
                .Bind(features => features
                    .Select(f => CreateHeritageSLAFeature(f.ToMicroBlueprint(), $"{f.Name}Ability"))
                    .Collect())
                .Map(features => features
                    .SelectMany(f => f)
                    .Select(f => f.Eval())
                    .ToArray());
        });

        static IInitContextBlueprint<BlueprintFeatureSelection> Create()
        {
            var features = new[]
            {
                NoAdditionalTraitsPrototype.Setup(
                    InitContext.NewBlueprint<BlueprintFeature>(
                        GeneratedGuid.Get("NoAdditionalAasimarTraits")))
                    .AddBlueprintDeferred(GeneratedGuid.NoAdditionalAasimarTraits),
                DeathlessSpirit.Create(),
                ExaltedResistance.Create(),
                CelestialCrusader.Create(),
                CrusadingMagic.Create()
            };

            var selection =
                InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get("AasimarFeatureSelection"))
                    .Combine(features.Collect())
                    .Map(bps =>
                    {
                        var (selection, features) = bps;

                        selection.m_DisplayName = Localized.DisplayName;
                        selection.m_Description = Localized.Description;

                        selection.Groups = [FeatureGroup.Racial];

                        selection.AddComponent<SelectionPriority>(c =>
                        {
                            c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                                CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures;

                            c.ActionPriority = LevelUpActionPriority.Heritage;
                        });

                        selection.AddFeatures(features);

                        return selection;
                    })
                    .AddBlueprintDeferred(GeneratedGuid.AasimarFeatureSelection);
            
            return selection;
        }

        [Init]
        static void Init()
        {
            InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.AasimarRace)
                .Combine(Create())
                .Map(bps =>
                {
                    var (race, selection) = bps;

                    race.m_Features = race.m_Features.Append(selection.ToReference<BlueprintFeatureBaseReference>()).ToArray();

                    return race;
                })
                .OnDemand(BlueprintsDb.Owlcat.BlueprintRace.AasimarRace.BlueprintGuid);
        }
    }
}
#endif