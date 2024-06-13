using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static partial class ComprehensiveEducation
    {
        [LocalizedString]
        public static readonly string DisplayName = "Comprehensive Education";

        [LocalizedString]
        public static readonly string Description =
            "Humans raised with skilled teachers draw upon vast swathes of knowledge gained over centuries " +
            "of civilization. They gain all Knowledge and Lore skills as " +
            $"{new Link(Page.Skills, "class skills")}, and they gain a +1 racial " +
            $"{new Link(Page.Bonus, "bonus")} on skill {new Link(Page.Check, "checks")} for " +
            "each Knowledge or Lore skill that they gain as a class skill from their class levels. This " +
            "racial trait replaces the Skilled trait.";

        internal static IInitContext<BlueprintFeature> Create() =>
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.ComprehensiveEducation)
                .Map((BlueprintFeature blueprint) =>
                {
                    blueprint.m_DisplayName = Localized.DisplayName;
                    blueprint.m_Description = Localized.Description;

                    foreach (var skill in new[]
                        {
                            StatType.SkillKnowledgeArcana,
                            StatType.SkillKnowledgeWorld,
                            StatType.SkillLoreNature,
                            StatType.SkillLoreReligion
                        })
                    {
                        blueprint.AddComponent<AddClassSkill>(acs => acs.Skill = skill);
                        blueprint.AddComponent<AddBackgroundClassSkill>(abcs => abcs.Skill = skill);
                    }

                    blueprint.SetIcon("95c4bb72353edb34082f088a5bd18cb2", 21300000);

                    blueprint.Groups = new[] { FeatureGroup.Racial };
                        
                    blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, hideInUI: false, removeOnApply: true);

                    return blueprint;
                })
                .AddOnTrigger(GeneratedGuid.ComprehensiveEducation, Triggers.BlueprintsCache_Init);
    }

}
