using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.InitContext;
using MicroWrath.Localization;
#if DEBUG
namespace AlternateRacialTraits.Features.Aasimar
{
    internal static partial class AasimarFeatureSelection
    {
        [LocalizedString]
        internal const string DisplayName = "Alternate Racial Traits";

        [LocalizedString]
        internal const string Description = "The following alternate traits are available";

        static IInitContextBlueprint<BlueprintFeatureSelection> Create()
        {
            var features = new[]
            {
                NoAdditionalTraitsPrototype.Setup(
                    InitContext.NewBlueprint<BlueprintFeature>(
                        GeneratedGuid.Get("NoAdditionalAasimarTraits")))
                    .AddBlueprintDeferred(GeneratedGuid.NoAdditionalAasimarTraits),
                DeathlessSpirit.Create(),
                ExaltedResistance.Create()
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