using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features.Oread;

internal static class OreadFeatureSelection
{
    [LocalizedString]
    public const string DisplayName = "Alternate Racial Traits";
    [LocalizedString]
    public const string Description = "The following alternate traits are available";

    [Init]
    internal static void Init()
    {
        var noTraits =
            NoAdditionalTraitsPrototype.Setup(
                InitContext.NewBlueprint<BlueprintFeature>(
                    GeneratedGuid.Get("NoAlternateOreadTraits")))
                .AddBlueprintDeferred(GeneratedGuid.NoAlternateOreadTraits);

        var graniteSkin = GraniteSkin.Create();
        var crystallineForm = CrystallineForm.Create();

        var context = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get(nameof(OreadFeatureSelection)))
            .Combine(BlueprintsDb.Owlcat.BlueprintRace.OreadRace)
            .Map(bps =>
            {
                var (selection, race) = bps;

                selection.m_DisplayName = LocalizedStrings.Features_Oread_OreadFeatureSelection_DisplayName;
                selection.m_Description = LocalizedStrings.Features_Oread_OreadFeatureSelection_Description;

                selection.Groups = [FeatureGroup.Racial];

                race.m_Features = race.m_Features.Append(selection.ToReference<BlueprintFeatureBaseReference>());

                return (selection, race);
            });

        _ = context.Map(pair => pair.race).AddOnTrigger(BlueprintsDb.Owlcat.BlueprintRace.OreadRace.BlueprintGuid, Triggers.BlueprintsCache_Init);

        _ = context.Map(pair => pair.selection)
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
            .AddOnTrigger(GeneratedGuid.OreadFeatureSelection, Triggers.BlueprintsCache_Init);
    }
}
