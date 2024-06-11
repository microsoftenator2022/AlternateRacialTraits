using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static partial class FocusedStudyProgression
    {
        [LocalizedString]
        public static readonly string DisplayName = "Focused Study";

        [LocalizedString]
        public static readonly string Description =
            "All humans are skillful, but some, rather than being generalists, tend to specialize in a " +
            $"handful of {new Link(Page.Skills, "skills")}. At 1st, 8th, and 16th level, such humans " +
            "gain Skill Focus in a skill of their choice as a bonus feat. This racial trait replaces the " +
            "bonus feat trait.";

        internal static IInitContext<BlueprintProgression> Create() =>
            InitContext.NewBlueprint<BlueprintProgression>(GeneratedGuid.FocusedStudyProgression)
                .Combine(BlueprintsDb.Owlcat.BlueprintFeatureSelection.SkillFocusSelection)
                .Map(bps =>
                {
                    var (progression, skillFocus) = bps;

                    progression.m_DisplayName = Localized.DisplayName;
                    progression.m_Description = Localized.Description;

                    progression.AddFeatures(1, skillFocus);
                    progression.AddFeatures(8, skillFocus);
                    progression.AddFeatures(16, skillFocus);

                    progression.SetIcon("42cb25b90b7c7d34e956c7822a9349cb", 21300000);

                    return progression;
                })
                .RegisterBlueprint(GeneratedGuid.FocusedStudyProgression, Triggers.BlueprintsCache_Init);
    }
}
