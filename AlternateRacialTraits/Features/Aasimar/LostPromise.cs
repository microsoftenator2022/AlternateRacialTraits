using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlternateRacialTraits.Features.Tiefling;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

using MicroWrath;
using MicroWrath.Components;
using MicroWrath.Deferred;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util;

using Newtonsoft.Json;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar;

internal static partial class LostPromise
{
    [LocalizedString]
    internal const string DisplayName = "Lost Promise";

    [LocalizedString]
    internal static readonly string Description =
        "While many view aasimars’ beauty and celestial powers as a gift, in some communities an aasimar might " +
        "be persecuted for being different and fall into darkness. The forces of evil delight in such a " +
        "perversion of their celestial counterparts’ gifts. As long as the aasimar retains an " +
        $"evil {new Link(Page.Alignment, "alignment")}, she gains the maw or claw tiefling " +
        $"alternate racial {new Link(Page.Trait, "trait")}. " +
        "This racial trait replaces the spell-like ability racial trait.";

    internal static IDeferredBlueprint<BlueprintParametrizedFeature> Create()
    {
        var selection = Deferred.NewBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.Get(nameof(LostPromise)))
            .Combine(MawOrClaw.MawFeature.Value)
            .Combine(MawOrClaw.ClawFeature.Value)
            .Combine(AasimarFeatureSelection.SLAPrerequisiteComponents())
            .Map(things =>
            {
                var (selection, maw, claw, slaPrerequisites) = things.Expand();

                selection.m_DisplayName = Localized.DisplayName;
                selection.m_Description = Localized.Description;

                selection.Groups = [FeatureGroup.Racial];

                selection.ParameterType = FeatureParameterType.FeatureSelection;

                selection.BlueprintParameterVariants =
                [
                    maw.ToReference<AnyBlueprintReference>(),
                    claw.ToReference<AnyBlueprintReference>()
                ];

                _ = selection.AddComponent<SelectionPriority>(c => c.ActionPriority = LevelUpActionPriority.Heritage);

                _ = selection.AddComponent<LostPromiseComponent>();

                _ = selection.AddComponents(slaPrerequisites);
                _ = selection.AddComponent<PrerequisiteAlignment>(c => c.Alignment = AlignmentMaskType.Evil);

                selection.ReapplyOnLevelUp = true;
                
                return selection;
            });

        return selection.AddBlueprintDeferred(GeneratedGuid.LostPromise);
    }

    internal class LostPromiseComponent : UnitFactComponentDelegate<LostPromiseComponent.Data>, IAlignmentChangeHandler
    {
        BlueprintUnitFact Feature => (this.Param.Blueprint as BlueprintUnitFact)!;

        void Apply()
        {
            if (!base.Owner.Alignment.ValueRaw.HasComponent(AlignmentComponent.Evil))
                return;

            if (!this.Owner.HasFact(this.Feature))
                base.Data.AppliedFact = this.Owner.AddFact(this.Feature);
        }

        void Remove()
        {
            if (base.Data.AppliedFact is null)
                return;

            this.Owner.RemoveFact(base.Data.AppliedFact);

            base.Data.AppliedFact = null;
        }

        public override void OnActivate() => this.Apply();

        public override void OnDeactivate() => this.Remove();

        public override void OnRecalculate() => this.Apply();

        public void HandleAlignmentChange(UnitEntityData unit, Alignment newAlignment, Alignment prevAlignment)
        {
            if (unit != base.Owner)
                return;

            if (newAlignment.HasComponent(AlignmentComponent.Evil) && !prevAlignment.HasComponent(AlignmentComponent.Evil))
            {
                this.Apply();
            }

            if (!newAlignment.HasComponent(AlignmentComponent.Evil) && prevAlignment.HasComponent(AlignmentComponent.Evil))
            {
                this.Remove();

                _ = EventBus.RaiseEvent<IAlignmentFeaturesChangeHandler>(
                    h => h.HandleAlignmentFeaturesLost(unit, newAlignment, prevAlignment),
                    true);
            }
        }

        public new class Data
        {
            [JsonProperty]
            public EntityFact? AppliedFact;
        }
    }
}