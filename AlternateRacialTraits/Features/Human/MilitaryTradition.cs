using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util.Linq;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static class MilitaryTradition
    {
        [LocalizedString]
        public static readonly string DisplayName = "Military Tradition";
        [LocalizedString]
        public static readonly string Description =
            "Several human cultures raise all children (or all children of a certain social class) to serve " +
            "in the military or defend themselves with force of arms. They gain " +
            $"{new Link(Page.Weapon_Proficiency, "proficiency")} with up to two martial or " +
            "exotic weapons appropriate to their culture. This racial trait replaces the bonus feat trait.";

        internal static BlueprintInitializationContext.ContextInitializer<(BlueprintFeatureSelection, BlueprintFeatureSelection)>
            Create(BlueprintInitializationContext context)
        {

            var firstSelection = context.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.MilitaryTraditionSelection,
                $"{nameof(MilitaryTradition)}.FirstSelection")
                .Map(selection =>
                {
                    selection.m_DisplayName = LocalizedStrings.Features_Human_MilitaryTradition_DisplayName;
                    selection.m_Description = LocalizedStrings.Features_Human_MilitaryTradition_Description;

                    selection.Groups = new[] { FeatureGroup.Racial };

                    return selection;
                });

            var secondSelection = context.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.MilitaryTraditionSecondSelection,
                $"{nameof(MilitaryTradition)}.SecondSelection")
                .Map(selection =>
                {
                    selection.m_DisplayName = LocalizedStrings.Features_Human_MilitaryTradition_DisplayName;
                    selection.m_Description = LocalizedStrings.Features_Human_MilitaryTradition_Description;

                    selection.Groups = new[] { FeatureGroup.Racial };

                    selection.AddComponent<PrerequisiteNoFeature>(c =>
                    {
                        c.m_Feature = selection.ToReference<BlueprintFeatureReference>();
                        c.HideInUI = true;
                    });

                    return selection;
                });

            return firstSelection
                .Combine(secondSelection)
                .Map(bps =>
                {
                    var (first, second) = bps;

                    var exotic = new MicroBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.ExoticWeaponProficiencyParametrized.ToString());
                    var martial = new MicroBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.MartialWeaponProficiencyParametrized.ToString());

                    first.AddFeatures(exotic, martial);
                    second.AddFeatures(exotic, martial);

                    second.AddPrerequisiteFeature(first.ToMicroBlueprint(), true, false);
                    second.HideNotAvailibleInUI = true;

                    return (first, second);
                });
        }
    }
}
