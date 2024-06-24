using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
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

#if DEBUG
namespace AlternateRacialTraits.Features.Tiefling;

internal static partial class TieflingFeatureSelection
{
    [LocalizedString]
    internal const string DisplayName = "Alternate Racial Traits";

    [LocalizedString]
    internal const string Description = "The following alternate traits are available";

    internal static IInitContextBlueprint<BlueprintFeatureSelection> Create()
    {
        var noTraits =
            NoAdditionalTraitsPrototype.Setup(
                InitContext.NewBlueprint<BlueprintFeature>(
                    GeneratedGuid.Get("NoAlternateTieflingTraits")))
                .AddBlueprintDeferred(GeneratedGuid.NoAlternateTieflingTraits);

        IEnumerable<IInitContextBlueprint<BlueprintFeature>> features =
        [
            noTraits,
            //BeguilingLiar.Create(),
            ScaledSkin.CreateResistanceSelection()
        ];

        var selection = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get("TieflingFeatureSelection"))
            .Combine(features.Collect())
            .Map(bps =>
            {
                var (selection, features) = bps;

                selection.m_DisplayName = Localized.DisplayName;
                selection.m_Description = Localized.Description;

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
#endif