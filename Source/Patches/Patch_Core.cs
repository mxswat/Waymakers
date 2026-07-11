using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Waymakers
{
    public static class Patch_CaravanGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Caravan __instance)
        {
            foreach (var g in __result)
                yield return g;

            var gizmo = CreateGizmo(__instance);
            if (gizmo != null)
                yield return gizmo;
        }

        private static Gizmo CreateGizmo(Caravan caravan)
        {
            var abilityDef = WaymakersMod.CoordinateWorksAbilityDef;
            if (abilityDef == null) return null;

            var pawns = CaravanUtils.GetAllCaravanPawns(caravan);

            Ability bestAbility = null;
            Pawn bestPawn = null;

            foreach (var pawn in pawns)
            {
                if (pawn.abilities == null || pawn.Downed) continue;
                var ability = pawn.abilities.AllAbilitiesForReading
                    .FirstOrDefault(a => a.def == abilityDef);
                if (ability == null) continue;

                bestAbility = ability;
                bestPawn = pawn;
                if (ability.CooldownTicksRemaining <= 0) break;
            }

            if (bestPawn == null) return null;

            bool onCooldown = bestAbility.CooldownTicksRemaining > 0;

            Command_Action cmd = new Command_Action();
            cmd.defaultLabel = abilityDef.label;
            cmd.defaultDesc = abilityDef.description;
            cmd.icon = abilityDef.uiIcon;
            cmd.Disabled = onCooldown;
            if (onCooldown)
            {
                cmd.disabledReason = "AbilityOnCooldown"
                    .Translate(bestAbility.CooldownTicksRemaining.ToStringTicksToPeriod());
            }
            cmd.action = delegate
            {
                var def = WaymakersMod.CoordinateWorksAbilityDef;
                foreach (var p in CaravanUtils.GetAllCaravanPawns(caravan))
                {
                    if (p.abilities == null || p.Downed) continue;
                    var a = p.abilities.AllAbilitiesForReading
                        .FirstOrDefault(x => x.def == def && x.CooldownTicksRemaining <= 0);
                    if (a == null) continue;
                    a.Activate(new LocalTargetInfo(p), new LocalTargetInfo(p));
                    break;
                }
            };
            return cmd;
        }
    }

    public static class Patch_OverlayRoad
    {
        public static void Postfix(PlanetTile fromTile, PlanetTile toTile, RoadDef roadDef)
        {
            if (roadDef == null) return;
            int tier = Patch_EndConstruction.RoadQualityTier(roadDef);
            int baseMood = new[] { 2, 5, 9, 13 }[tier];
            int mood = baseMood;
            int durationDays = 5;
            if (tier >= 2) durationDays *= 2;
            Patch_EndConstruction.ApplyThoughtToWaymakers(tier, mood, durationDays);
        }
    }

    public static class Patch_MoteColor
    {
        private static readonly FieldInfo MoteField =
            AccessTools.Field(typeof(HediffComp_GiveHediffsInRange), "mote");

        public static void Postfix(HediffComp_GiveHediffsInRange __instance)
        {
            if (__instance.parent.def != WaymakersMod.CoordinateWorksHediff) return;
            var mote = (Mote)MoteField.GetValue(__instance);
            if (mote != null && !mote.Destroyed)
                mote.instanceColor = __instance.parent.pawn.Ideo?.Color ?? Color.white;
        }
    }
}
