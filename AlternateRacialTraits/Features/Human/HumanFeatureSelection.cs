using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Class.LevelUp;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Constructors;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features.Human
{
    internal static class NoAdditionalTraits
    {
        [LocalizedString]
        public static readonly string DisplayName = "None";
        [LocalizedString]
        public static readonly string Description = "No alternate trait";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintFeature>(GeneratedGuid.NoAdditionaHumanTraits, nameof(NoAdditionalTraits))
                .Map((BlueprintFeature feat) =>
                {
                    feat.m_DisplayName = LocalizedStrings.Features_Human_NoAdditionalTraits_DisplayName;
                    feat.m_Description = LocalizedStrings.Features_Human_NoAdditionalTraits_Description;
                        
                    feat.HideInUI = true;
                    feat.HideInCharacterSheetAndLevelUp = true;

                    feat.Groups = new[] { FeatureGroup.Racial }; 

                    return feat;
                });
    }

    internal static class HumanFeatureSelection
    {
        [LocalizedString]
        public static readonly string DisplayName = "Alternate Racial Traits";
        [LocalizedString]
        public static readonly string Description = "The following alternate traits are available";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeatureSelection> Create(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.HumanFeatureSelection, nameof(HumanFeatureSelection))
                .Map(((BlueprintFeatureSelection selection) =>
                {
                    MicroLogger.Debug(() => $"Setting up {nameof(HumanFeatureSelection)}");

                    selection.m_DisplayName = LocalizedStrings.Features_Human_HumanFeatureSelection_DisplayName;
                    selection.m_Description = LocalizedStrings.Features_Human_HumanFeatureSelection_Description;

                    selection.Groups = new[] { FeatureGroup.Racial };

                    selection.AddComponent<OverrideSelectionPriority>(c =>
                        c.Priority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                            CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures - 1);

                    return selection;
                }));

        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            var bonusFeatDummy = initContext.NewBlueprint<BlueprintFeature>(
                GeneratedGuid.BasicFeatSelectionDummy, nameof(GeneratedGuid.BasicFeatSelectionDummy));

            var noMoreSelections = NoAdditionalTraits.Create(initContext);
            var selection = HumanFeatureSelection.Create(initContext);

            selection
                .Combine(bonusFeatDummy)
                .Combine(HumanBonusFeat.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), bonusFeat) = bps;

                    bonusFeat.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), true, true);

                    dummy.SetIcon(bonusFeat.Icon);
                    
                    dummy.m_DisplayName = bonusFeat.m_DisplayName;
                    dummy.m_Description = bonusFeat.m_Description;

                    selection.AddFeatures(bonusFeat.ToMicroBlueprint());

                    return (selection, dummy.ToMicroBlueprint());
                })
                .Combine(noMoreSelections)
                .Map(bps =>
                {
                    var ((selection, dummy), noTraits) = bps;

                    noTraits.AddComponent<PrerequisiteNoFeature>(pnf =>
                    {
                        pnf.m_Feature = dummy.ToReference<BlueprintFeature, BlueprintFeatureReference>();

                        pnf.HideInUI = true;
                    });

                    selection.AddFeatures(noTraits.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(AwarenessFeature.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), awareness) = bps;

                    awareness.AddPrerequisiteFeature(dummy, true, true);

                    selection.AddFeatures(awareness.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(ComprehensiveEducation.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), comprehensiveEducation) = bps;

                    selection.AddFeatures(comprehensiveEducation.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(GiantAncestry.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), giantAncestry) = bps;

                    selection.AddFeatures(giantAncestry.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(HistoryOfTerrorsTrait.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), historyOfTerrors) = bps;

                    selection.AddFeatures(historyOfTerrors.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(PracticedHunter.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), practicedHunter) = bps;

                    selection.AddFeatures(practicedHunter.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(UnstoppableMagic.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), unstoppableMagic) = bps;

                    unstoppableMagic.AddPrerequisiteFeature(dummy, true, true);

                    selection.AddFeatures(unstoppableMagic.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(FocusedStudyProgression.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), focusedStudy) = bps;

                    focusedStudy.AddPrerequisiteFeature(dummy, true, true);

                    selection.AddFeatures(focusedStudy.ToMicroBlueprint());

                    return (selection, dummy);
                })
                .Combine(DualTalent.Create(initContext))
                .Map(bps =>
                {
                    var ((selection, dummy), dualTalent) = bps;

                    dualTalent.AddPrerequisiteFeature(dummy, true, true);
                    dualTalent.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, false, true);

                    selection.AddFeatures(dualTalent.ToMicroBlueprint());

                    return (selection, dummy);
                })

                .Combine(initContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.HumanRace))
                .Combine(noMoreSelections)
                .Map(bps =>
                {
                    MicroLogger.Debug(() => "Updating human features");

                    var (((selection, dummy), humanRace), noMoreSelections) = bps;

                    var features = new List<BlueprintFeatureBase> { selection, dummy.GetBlueprint()! };

                    features.AddRange(humanRace.Features
                        .Where(f => f != BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection.GetBlueprint()));

                    humanRace.m_Features = features
                        .Select(bp => bp.ToReference<BlueprintFeatureBaseReference>())
                        .ToArray();

                    var selectionRef = selection.ToReference<BlueprintFeatureBaseReference>();

                    foreach (var f in selection.m_AllFeatures.Where(f => f.Get() != noMoreSelections))
                    {
                        f.Get().AddComponent(new UnitFactActivateEvent(e =>
                        {
                            Util.AddRacialSelection(e.Owner, new [] { selectionRef.Get() });
                        }));
                    }

                    return (selection, dummy);
                })

                .Register();
        }
    }
}
