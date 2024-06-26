using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features.Tiefling;

internal static partial class TieflingFeatureSelection
{
    [LocalizedString]
    internal const string DisplayName = "Alternate Racial Traits";

    [LocalizedString]
    internal const string Description = "The following alternate traits are available";

    internal static IInitContext<IEnumerable<BlueprintFeature>> TieflingHeritageFeatures =>
        InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeatureSelection.TieflingHeritageSelection)
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
        return TieflingHeritageFeatures
            .Bind(features => features
                .Select(f => HeritageFeatures.CreateSkillFeature(f.ToMicroBlueprint(), $"{f.name}Skilled", f.m_DisplayName))
                .Collect())
            .Map(Enumerable.ToArray);
    });

    internal static IInitContext<BlueprintComponent[]> SkilledPrerequisiteComponents() => HeritageFeatures.SkilledPrerequisiteComponents(SkilledFeatures.Value);

    internal static readonly Lazy<IInitContext<(BlueprintFeature feature, BlueprintAbility[] facts)[]>> SLAFeatures = new(() =>
    {
        return TieflingHeritageFeatures
            .Bind(features => features
                .Select(f => HeritageFeatures.CreateHeritageSLAFeature(f.ToMicroBlueprint(), $"{f.name}Ability", f.m_DisplayName))
                .Collect(f => f))
            .Bind(fs => fs.Collect())
            .Map(Enumerable.ToArray);
    });

    internal static IInitContext<BlueprintComponent[]> SLAPrerequisiteComponents() => HeritageFeatures.SLAPrerequisiteComponents(SLAFeatures.Value);

    internal static IInitContextBlueprint<BlueprintFeatureSelection> Create()
    {
        var noTraits =
            NoAdditionalTraitsPrototype.Setup(
                InitContext.NewBlueprint<BlueprintFeature>(
                    GeneratedGuid.Get("NoAlternateTieflingTraits")))
                .AddBlueprintDeferred(GeneratedGuid.NoAlternateTieflingTraits);

        IEnumerable<IInitContextBlueprint<BlueprintFeature>> features =
        [
            BeguilingLiar.Create(),
            ScaledSkin.CreateResistanceSelection(),
            MawOrClaw.Create(),
            SmiteGood.Create()
        ];

        var selection = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get("TieflingFeatureSelection"))
            .Combine(noTraits)
            .Combine(features.Collect())
            .Map(bps =>
            {
                var (selection, noTraits, features) = bps.Flatten();

                selection.m_DisplayName = Localized.DisplayName;
                selection.m_Description = Localized.Description;

                selection.AddFeatures(noTraits);

                foreach (var feature in features)
                {
                    feature.AddComponent(new UnitFactActivateEvent(e =>
                    {
                        Util.AddLevelUpSelection(e.Owner, [selection.ToReference<BlueprintFeatureBaseReference>()], e.Owner.Progression.Race);
                    }));
                }

                selection.AddFeatures(features);

                selection.Groups = [FeatureGroup.Racial];

                _ = selection.AddComponent<SelectionPriority>(c =>
                {
                    c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                        CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures;

                    c.ActionPriority = LevelUpActionPriority.Heritage;
                });

                return selection;
            })
            .AddBlueprintDeferred(GeneratedGuid.TieflingFeatureSelection);

        _ = InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.TieflingRace)
            .Combine(selection)
            .Map(bps =>
            {
                var (race, selection) = bps;

                race.m_Features = race.m_Features.Append(selection.ToReference<BlueprintFeatureBaseReference>());

                return race;
            })
            .OnDemand(BlueprintsDb.Owlcat.BlueprintRace.TieflingRace);

        return selection;
    }

    [Init]
    static void Init() => _ = Create();
}