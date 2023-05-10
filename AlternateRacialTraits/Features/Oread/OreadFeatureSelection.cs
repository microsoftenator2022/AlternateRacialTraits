using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features.Oread
{
    internal static class NoAlternateTraits
    {
        [LocalizedString]
        public const string DisplayName = "None";
        [LocalizedString]
        public const string Description = "No alternate trait";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("NoAlternateOreadTraits"), nameof(GeneratedGuid.NoAlternateOreadTraits))
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_Oread_NoAlternateTraits_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Oread_NoAlternateTraits_Description;

                    blueprint.HideInUI = true;
                    blueprint.HideInCharacterSheetAndLevelUp = true;

                    return blueprint;
                });
    }

    internal static class OreadFeatureSelection
    {
        [LocalizedString]
        public const string DisplayName = "Alternate Racial Traits";
        [LocalizedString]
        public const string Description = "The following alternate traits are available";

        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            var noTraits = NoAlternateTraits.Create(initContext);
            var graniteSkin = GraniteSkin.Create(initContext);
            var crystallineForm = CrystallineForm.Create(initContext);

            var selection = initContext.NewBlueprint<BlueprintFeatureSelection>(
                GeneratedGuid.Get(nameof(OreadFeatureSelection)), nameof(OreadFeatureSelection))
                .Map(selection =>
                {
                    selection.m_DisplayName = LocalizedStrings.Features_Oread_OreadFeatureSelection_DisplayName;
                    selection.m_Description = LocalizedStrings.Features_Oread_OreadFeatureSelection_Description;

                    selection.Groups = new[] { FeatureGroup.Racial };

                    var race = BlueprintsDb.Owlcat.BlueprintRace.OreadRace.GetBlueprint()!;

                    race.m_Features = race.m_Features.Append(selection.ToReference<BlueprintFeatureBaseReference>());

                    return selection;
                });

            selection
                .Combine(noTraits)
                .Combine(graniteSkin)
                .Combine(crystallineForm)
                .Map(bps =>
                {
                    var (selection, noTraits, graniteSkin, crystallineForm) = bps.Flatten();

                    selection.AddFeatures(noTraits.ToMicroBlueprint());
                    selection.AddFeatures(graniteSkin.ToMicroBlueprint());
                    selection.AddFeatures(crystallineForm.ToMicroBlueprint());

                    return selection;
                })
                .Register();
        }
    }
}
