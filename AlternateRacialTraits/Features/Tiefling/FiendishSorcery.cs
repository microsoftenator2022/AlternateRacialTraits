using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Deferred;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

using Newtonsoft.Json;

namespace AlternateRacialTraits.Features.Tiefling;
internal static partial class FiendishSorcery
{
    [LocalizedString]
    internal const string DisplayName = "Fiendish Sorcery";

    internal static readonly Lazy<IDeferredBlueprint<BlueprintFeature>> FiendishSorceryFeature = new(() =>
    {
        var effectFeature = Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("FiendishSorceryEffect"))
            .Map(bp =>
            {
                bp.HideInUI = true;

                return bp;
            })
            .AddBlueprintDeferred(GeneratedGuid.FiendishSorceryEffect);

        var feature = Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("FiendishSorcery"))
            .Combine(effectFeature)
            .Combine(Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.TieflingHeritageClassic))
            .Combine(Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.TieflingHeritageDevil))
            .Combine(Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.AbyssalBloodlineRequisiteFeature))
            .Combine(Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.InfernalBloodlineRequisiteFeature))
            .Combine(Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeatureSelection.TieflingHeritageSelection))
            .Map(bps =>
            {
                var (feature, effectFeature, classic, devil, abyssal, infernal, heritageSelection) = bps.Expand();

                feature.m_DisplayName = Localized.DisplayName;
                feature.HideInUI = true;

                feature.Groups = [FeatureGroup.Racial];

                var component = feature.AddComponent<AddConditionalFact>();

                component.Feature = effectFeature.ToReference<BlueprintUnitFactReference>();

                static HasFact ownerHasFact(BlueprintUnitFact blueprint) =>
                    Conditions.HasFact(c =>
                    {
                        c.Unit = new FactOwner();
                        c.m_Fact = blueprint.ToReference();
                    });

                _ = component.Conditions.Add(
                    ownerHasFact(classic).And(ownerHasFact(abyssal).Or(ownerHasFact(infernal))),
                    ownerHasFact(devil).And(ownerHasFact(infernal)));

                component.Conditions.Operation = Operation.Or;

                _ = feature.AddRecalculateOnFactsChange(c => c.m_CheckedFacts =
                [
                    abyssal.ToReference<BlueprintUnitFactReference>(),
                    infernal.ToReference<BlueprintUnitFactReference>()
                ]);

                foreach (var bp in new[] {classic, devil})
                {
                    bp.GetComponent<ContextRankConfig>().m_FeatureList = [effectFeature.ToReference()];
                    bp.GetComponent<RecalculateOnFactsChange>().m_CheckedFacts = [effectFeature.ToReference<BlueprintUnitFactReference>()];
                }

                _ = heritageSelection.AddAddFacts(c => c.m_Facts = [feature.ToReference<BlueprintUnitFactReference>()]);

                return feature;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.FiendishSorcery);
    });
}

[AllowedOn(typeof(BlueprintUnitFact))]
internal class AddConditionalFact : UnitFactComponentDelegate<AddConditionalFact.Data>, IOwnerGainLevelHandler
{
    public ConditionsChecker Conditions = new();

    public BlueprintUnitFactReference? Feature;

    void Apply()
    {
        if (base.Data.AppliedFact is not null)
            return;

        if (!Conditions.Check())
        {
            //this.OnDeactivate();
            return;
        }

        base.Data.AppliedFact = base.Owner.AddFact(this.Feature?.Get());
    }

    public void HandleUnitGainLevel() => this.Apply();

    public override void OnActivate() => this.Apply();
    
    public override void OnDeactivate()
    {
        if (base.Data.AppliedFact is null)
            return;

        base.Owner.RemoveFact(base.Data.AppliedFact);
        base.Data.AppliedFact = null;
    }

    public override void OnRecalculate() => this.Apply();

    public new class Data
    {
        [JsonProperty]
        public EntityFact? AppliedFact;
    }
}
