﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Localization;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human;

internal static partial class AwarenessFeature
{
    [LocalizedString]
    public static readonly string DisplayName = "Awareness";

    [LocalizedString]
    public static readonly string Description =
        "Humans raised within monastic traditions or communities that encourage mindfulness seem to " +
        "shrug off many dangers more easily than other humans. They gain a +1 racial " +
        $"{new Link(Page.Bonus, "bonus")} on all " +
        $"{new Link(Page.Saving_Throw, "saving throws")} and " +
        $"{new Link(Page.Concentration, "concentration checks")}. This racial trait replaces " +
        "humans’ bonus feat.";

    internal static IInitContext<BlueprintFeature> Create() =>
        InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Awareness, "AwarenessFeature")
            .Map((BlueprintFeature blueprint) =>
            {
                blueprint.m_DisplayName = Localized.DisplayName;
                blueprint.m_Description = Localized.Description;

                blueprint.Groups = [FeatureGroup.Racial];

                // JudjemntProtection
                blueprint.SetIcon("af5df55819255b54fb3491bbd67a569e", 21300000);

                _ = blueprint.AddBuffAllSavesBonus(component =>
                {
                    component.Descriptor = ModifierDescriptor.Racial;
                    component.Value = 1;
                });

                _ = blueprint.AddConcentrationBonus(component =>
                {
                    component.Value = 1;
                });

                return blueprint;
            })
            .AddBlueprintDeferred(GeneratedGuid.Awareness);
}
