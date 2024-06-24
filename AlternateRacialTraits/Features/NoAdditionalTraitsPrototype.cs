using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

using MicroWrath;
using MicroWrath.Extensions;
using MicroWrath.InitContext;
using MicroWrath.Localization;

namespace AlternateRacialTraits.Features;

internal static partial class NoAdditionalTraitsPrototype
{
    [LocalizedString]
    public static readonly string DisplayName = "None";
    [LocalizedString]
    public static readonly string Description = "No alternate trait";

    internal static IInitContext<BlueprintFeature> Setup(IInitContext<BlueprintFeature> blueprint) =>
        blueprint
            .Map((BlueprintFeature feat) =>
            {
                feat.m_DisplayName = Localized.DisplayName;
                feat.m_Description = Localized.Description;

                feat.HideInUI = true;
                feat.HideInCharacterSheetAndLevelUp = true;

                _ = feat.AddComponent<NoTraitsComponent>();

                return feat;
            });
}

internal class NoTraitsComponent() : BlueprintComponent { }