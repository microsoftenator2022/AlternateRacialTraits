using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Oread
{
    //using InitFeature = BlueprintInitializationContext.ContextInitializer<BlueprintFeature>;

    internal static partial class CrystallineForm
    {
        [LocalizedString]
        public const string DisplayName = "Crystalline Form";
        [LocalizedString]
        public static readonly string Description =
            $"Oreads with this trait gain a +2 racial {new Link(Page.Bonus, "bonus")} to " +
            $"{new Link(Page.Armor_Class, "AC")} against rays thanks to their reflective crystalline " +
            "skin. In addition, once per day, they can deflect a single ray attack targeted at them as if they " +
            "were using the Deflect Arrows feat. This racial trait replaces earth affinity.";

        private interface IUnitDeflectedRayHandler : IGlobalSubscriber
        {
            void HandleUnitDeflectedRay(UnitEntityData unit, Projectile projectile);
        }

        internal class DeflectRay : UnitFactComponentDelegate<DeflectRay.ComponentData>,
            IUnitEquipmentHandler, IUnitActiveEquipmentSetHandler, IUnitAbilityResourceHandler, IUnitDeflectedRayHandler
        {
            public class ComponentData
            {
                public bool Enabled;
            }

            public BlueprintItemWeaponReference RayWeapon = null!;

            public BlueprintAbilityResourceReference Resource = null!;

            int ResourceAmount
            {
                get
                {
                    if (base.Owner is null || Resource?.Get() is null)
                        return 0;

                    var amount = base.Owner.Resources.GetResourceAmount(Resource.Get());

                    return amount;
                }
            }

            void UpdateState()
            {
                var hasFreeHand = base.Owner.Body.HandsAreEnabled &&
                    (!base.Owner.Body.PrimaryHand.HasItem || !base.Owner.Body.SecondaryHand.HasItem);

                var amount = this.ResourceAmount;

                MicroLogger.Debug(() => $"Resources: {amount}");

                if (amount > 0 && hasFreeHand && !base.Data.Enabled)
                {
                    base.Owner.State.Features.DeflectArrows.Retain();
                    base.Data.Enabled = true;
                }
                else if (amount <= 0 || (!hasFreeHand && base.Data.Enabled))
                {
                    base.Owner.State.Features.DeflectArrows.Release();
                    base.Data.Enabled = false;
                }

                MicroLogger.Debug(() => $"Deflect enabled? {base.Data.Enabled}");
            }

            public override void OnTurnOn() => UpdateState();
            public override void OnTurnOff()
            {
                if (base.Data.Enabled)
                {
                    base.Owner.State.Features.DeflectArrows.Release();
                    base.Data.Enabled = false;
                }
            }

            public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
            {
                if (slot.Owner == base.Owner && slot is HandSlot)
                    this.UpdateState();
            }
            public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
            {
                if (unit == base.Owner)
                    this.UpdateState();
            }
            public void HandleAbilityResourceChange(UnitEntityData unit, UnitAbilityResource resource, int oldAmount)
            {
                if (unit != base.Owner)
                    return;

                if (Resource?.Get() is not { } resourceBlueprint || resource.Blueprint != resourceBlueprint)
                    return;

                MicroLogger.Debug(() => $"{resource.Blueprint} {oldAmount} -> {resource.Amount}");

                this.UpdateState();
            }

            public void HandleUnitDeflectedRay(UnitEntityData unit, Projectile _)
            {
                if (unit != base.Owner) return;

                if (Resource?.Get() is { } resource) 
                    base.Owner.Resources.Spend(resource, 1);
            }

            [HarmonyPatch]
            class Patches
            {
                static bool CanDeflectRayWeapon(Projectile projectile)
                {
                    if (projectile.Target.IsUnit)
                    {
                        var components = projectile.Target.Unit.Facts.List.SelectMany(f => f.Components.Where(c => c.SourceBlueprintComponent is DeflectRay));

                        foreach (var c in components)
                        {
                            var data = c.GetData<DeflectRay.ComponentData>();
                            if (data is null)
                            {
                                MicroLogger.Debug(() => $"{nameof(DeflectRay)}.Data is null for {projectile.Blueprint} {projectile}");
                                continue;
                            }

                            MicroLogger.Debug(() => $"Deflect Ray is {(data.Enabled ? "Enabled" : "Disabled")}");

                            //MicroLogger.Debug(sb =>
                            //{
                            //    sb.AppendLine($"Projectile {projectile.Blueprint} {projectile}");
                            //    sb.Append($"AttackRoll is null? {projectile.AttackRoll is null}");

                            //    if (projectile.AttackRoll is null)
                            //        return;

                            //    sb.AppendLine();
                            //    sb.AppendLine($"AttackRoll.Weapon is null? {projectile.AttackRoll.Weapon is null}");

                            //    sb.AppendLine($"Reason: {projectile.AttackRoll.Reason?.Name}");
                            //    sb.AppendLine($"Fact: {projectile.AttackRoll.Reason?.Fact?.Blueprint}");
                            //    sb.Append($"Ability: {projectile.AttackRoll.Reason?.Ability?.Blueprint}");
                            //});

                            var projectileWeapon = projectile.AttackRoll?.Weapon?.MainWeapon?.Blueprint;

                            projectileWeapon ??= projectile.AttackRoll?.Reason?.Ability?.Blueprint
                                ?.GetComponent<AbilityDeliverProjectile>()
                                ?.Weapon;

                            var rayWeapon = (c.SourceBlueprintComponent as DeflectRay)?.RayWeapon?.Get();

                            //MicroLogger.Debug(() => $"Projectile weapon {projectileWeapon} == ray weapon {rayWeapon}? {projectileWeapon == rayWeapon}");

                            return
                                rayWeapon is not null &&
                                projectileWeapon == rayWeapon &&
                                data.Enabled;
                        }
                    }

                    return false;
                }

                static bool IsFromWeaponOrCanDeflectRay(Projectile projectile)
                {
                    return CanDeflectRayWeapon(projectile) || projectile.IsFromWeapon;
                }

                [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.CannotBeDeflected))]
                [HarmonyTranspiler]
                static IEnumerable<CodeInstruction> UnitCombatState_CannotBeDeflected_Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    var call = instructions.FirstOrDefault(ci =>
                        (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) &&
                        (ci.operand as MethodInfo) == AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.IsFromWeapon)));

                    if (call is not null)
                    {
                        call.operand = AccessTools.Method(typeof(Patches), nameof(IsFromWeaponOrCanDeflectRay));
                    }
                    else
                        MicroLogger.Error($"Cannot locate call to {nameof(Projectile)}.{nameof(Projectile.IsFromWeapon)}");

                    return instructions;
                }

                [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.CannotBeDeflected))]
                [HarmonyPostfix]
                static bool UnitCombatState_CannotBeDeflected_Postfix(bool __result, TimeSpan now, Projectile projectile, UnitEntityData attacker, UnitCombatState __instance)
                {
                    MicroLogger.Debug(() => $"!this.Unit.Descriptor.State.Features.DeflectArrows? {!__instance.Unit.Descriptor.State.Features.DeflectArrows}");
                    MicroLogger.Debug(() => $"now - this.m_LastDeflectArrowTime < 1.Rounds().Seconds? {now - __instance.m_LastDeflectArrowTime < 1.Rounds().Seconds}");
                    MicroLogger.Debug(() => $"projectile.AttackRoll.Weapon.Blueprint.IsNatural? {projectile.AttackRoll.Weapon.Blueprint.IsNatural}");
                    MicroLogger.Debug(() => "(attacker != null && Rulebook.Trigger<RuleCheckTargetFlatFooted>(new RuleCheckTargetFlatFooted(attacker, this.Unit)).IsFlatFooted)? " +
                    $"{(attacker != null && Rulebook.Trigger<RuleCheckTargetFlatFooted>(new RuleCheckTargetFlatFooted(attacker, __instance.Unit)).IsFlatFooted)}");

//#if !DEBUG
                    if (!__result && projectile.Target.Unit.IsAlly(attacker) && projectile.AttackRoll.AutoHit)
                        __result = true;
//#endif

                    MicroLogger.Debug(() => $"Can be deflected? {!__result}");

                    return __result;
                }

                [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.TryDeflectArrow))]
                [HarmonyPostfix]
                static void UnitCombatState_TryDeflectArrow_Postfix(bool __result, Projectile projectile)
                {
                    if (__result)
                        EventBus.RaiseEvent<IUnitDeflectedRayHandler>(handler => handler.HandleUnitDeflectedRay(projectile.Target.Unit, projectile));
                }

//#if DEBUG
//                [HarmonyPatch(typeof(Projectile), nameof(Projectile.TryDeflectArrow))]
//                [HarmonyPostfix]
//                static bool Projectile_TryDeflectArrow_Postfix(bool __result, Projectile __instance)
//                {
//                    MicroLogger.Debug(() => $"Deflected? {__result}");

//                    MicroLogger.Debug(() => $"AutoHit? {__instance.AttackRoll.AutoHit}");

//                    if (__result && __instance.AttackRoll.AutoHit)
//                    {
//                        //if (__instance.AttackRoll.Target.IsAlly(__instance.AttackRoll.Initiator))
//                        //    return false;

//                        __instance.AttackRoll.SetFake(AttackResult.Miss);
//                        __instance.AttackResult = AttackResult.Miss;
//                        return true;
//                    }
                    
//                    return __result;
//                }
//#endif

                [HarmonyPatch(typeof(Projectile), nameof(Projectile.AddForceToArrrow))]
                [HarmonyPrefix]
                static bool Projectile_AddForceToArrow_Prefix(Projectile __instance)
                {
                    return __instance.Blueprint.DeflectedArrowPrefab is not null;
                }
            }
        }

        internal static IInitContext<BlueprintFeature> Create()
        {
            var resource = InitContext.NewBlueprint<BlueprintAbilityResource>(GeneratedGuid.Get("CrystallineFormResource"))
                .Map(resource =>
                {
                    resource.m_MaxAmount = new() { BaseValue = 1 };

                    return resource;
                })
                .AddOnTrigger(GeneratedGuid.CrystallineFormResource, Triggers.BlueprintsCache_Init);

            var feature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("CrystallineForm"))
                .Combine(resource)
                .Combine(BlueprintsDb.Owlcat.BlueprintItemWeapon.RayItem)
                .Map(bps =>
                {
                    (BlueprintFeature feature, var resource, var ray) = bps.Expand();

                    feature.m_DisplayName = Localized.DisplayName;
                    feature.m_Description = Localized.Description;

                    feature.SetIcon("b3355206cc666b543addf8d60df20299", 21300000);

                    feature.Groups = [FeatureGroup.Racial];

                    feature.AddACBonusAgainstWeaponCategory(c =>
                    {
                        c.ArmorClassBonus = 2;
                        c.Category = WeaponCategory.Ray;

                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    feature.AddComponent<DeflectRay>(deflect =>
                    {
                        deflect.RayWeapon = ray.ToReference();
                        deflect.Resource = resource.ToReference();
                    });

                    feature.AddAddAbilityResources(addResources =>
                    {
                        addResources.Amount = 1;
                        addResources.m_Resource = resource.ToReference();
                    });

                    feature.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.AcidAffinityOread, true);

                    return feature;
                })
                .AddOnTrigger(GeneratedGuid.CrystallineForm, Triggers.BlueprintsCache_Init);

            return feature;
        }
    }
}
