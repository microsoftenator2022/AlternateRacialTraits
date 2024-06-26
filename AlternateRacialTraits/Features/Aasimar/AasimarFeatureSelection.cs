using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
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
using MicroWrath.Deferred;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

using UniRx;

namespace AlternateRacialTraits.Features.Aasimar;

internal static partial class AasimarFeatureSelection
{
    [LocalizedString]
    internal const string DisplayName = "Alternate Racial Traits";

    [LocalizedString]
    internal const string Description = "The following alternate traits are available";

    internal static IDeferred<IEnumerable<BlueprintFeature>> AasimarHeritageFeatures =>
        Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeatureSelection.AasimarHeritageSelection)
            .Bind(selection =>
            {
                var features = selection.AllFeatures;

                return features
                    .Select(f => Deferred.GetBlueprint(f.ToMicroBlueprint()))
                    .Collect();
            })
            .Map(features => features.NotNull());

    internal static readonly Lazy<IDeferred<BlueprintFeature[]>> SkilledFeatures = new(() =>
    {
        return AasimarHeritageFeatures
            .Bind(features => features
                .Select(f => HeritageFeatures.CreateSkillFeature(f.ToMicroBlueprint(), $"{f.name}Skilled", f.m_DisplayName))
                .Collect())
            .Map(Enumerable.ToArray);
    });

    internal static IDeferred<BlueprintComponent[]> SkilledPrerequisiteComponents() => HeritageFeatures.SkilledPrerequisiteComponents(SkilledFeatures.Value);

    internal static readonly Lazy<IDeferred<(BlueprintFeature feature, BlueprintAbility[] facts)[]>> SLAFeatures = new(() =>
    {
        return AasimarHeritageFeatures
            .Bind(features => features
                .Select(f => HeritageFeatures.CreateHeritageSLAFeature(f.ToMicroBlueprint(), $"{f.name}Ability", f.m_DisplayName))
                .Collect(f => f))
            .Bind(fs => fs.Collect())
            .Map(Enumerable.ToArray);
    });

    internal static IDeferred<BlueprintComponent[]> SLAPrerequisiteComponents() => HeritageFeatures.SLAPrerequisiteComponents(SLAFeatures.Value);
    
    static IDeferredBlueprint<BlueprintFeatureSelection> Create()
    {
        var features = new[]
        {
            DeathlessSpirit.Create(),
            ExaltedResistance.Create(),
            CelestialCrusader.Create(),
            CrusadingMagic.Create(),
            Heavenborn.Create(),
            LostPromise.Create()
        };

        var selection =
            Deferred.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get("AasimarFeatureSelection"))
                .Combine(NoAdditionalTraitsPrototype.Setup(
                    Deferred.NewBlueprint<BlueprintFeature>(
                        GeneratedGuid.Get("NoAdditionalAasimarTraits")))
                .AddBlueprintDeferred(GeneratedGuid.NoAdditionalAasimarTraits))
                .Combine(features.Collect())
                .Map(bps =>
                {
                    var (selection, noTraits, features) = bps.Flatten();

                    selection.m_DisplayName = Localized.DisplayName;
                    selection.m_Description = Localized.Description;

                    selection.Groups = [FeatureGroup.Racial];

                    _ = selection.AddComponent<SelectionPriority>(c =>
                    {
                        c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                            CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures;

                        c.ActionPriority = LevelUpActionPriority.Heritage;
                    });

                    foreach (var feature in features)
                    {
                        feature.AddComponent(new UnitFactActivateEvent(e =>
                        {
                            Util.AddLevelUpSelection(e.Owner, [selection.ToReference<BlueprintFeatureBaseReference>()], e.Owner.Progression.Race);
                        }));
                    }

                    selection.AddFeatures(noTraits);

                    selection.AddFeatures(features);

                    return selection;
                })
                .AddBlueprintDeferred(GeneratedGuid.AasimarFeatureSelection);
        
        return selection;
    }

    [Init]
    static void Init()
    {
        _ = Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.AasimarRace)
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