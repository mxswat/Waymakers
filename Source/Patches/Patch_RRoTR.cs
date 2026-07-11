using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Waymakers
{
    public static class Patch_EndConstruction
    {
        public static void Prefix(object __instance, Caravan caravan)
        {
            try
            {
                var site = Traverse.Create(__instance).Field("parent").GetValue();
                if (site == null) return;

                var roadDef = Traverse.Create(site).Field("roadDef").GetValue() as RoadDef;
                if (roadDef == null) return;

                int legs = LegCountTracker.Pop(site);
                int qualityTier = RoadQualityTier(roadDef);

                int baseMood = new[] { 2, 5, 9, 13 }[qualityTier];
                int mood = baseMood + (legs / 3);
                int durationDays = 5 + legs;
                if (qualityTier >= 2) durationDays *= 2;

                Log.Message($"[Waymakers] Road completed: {roadDef.label}, legs={legs}, qualityTier={qualityTier}, mood=+{mood}, duration={durationDays}d");

                var comp = Current.Game?.GetComponent<WaymakersGameComponent>();
                if (comp != null)
                    comp.LastRoadBuildTick = Find.TickManager.TicksGame;

                ApplyThoughtToWaymakers(qualityTier, mood, durationDays);
            }
            catch (Exception e)
            {
                Log.Error($"[Waymakers] Error in EndConstruction prefix: {e}");
            }
        }

        public static int RoadQualityTier(RoadDef roadDef)
        {
            string name = roadDef.defName;

            if (name.Contains("Glitter")) return 3;
            if (name.Contains("Asphalt")) return 2;
            if (name.StartsWith("Rail"))  return 2;
            if (name.Contains("Stone"))   return 1;

            float mcm = roadDef.movementCostMultiplier;
            if (mcm <= 0.5f) return 1;
            return 0;
        }

        private static readonly string[] TierDefNames = { "WM_RoadBuilt_Crude", "WM_RoadBuilt_Basic", "WM_RoadBuilt_Engineered", "WM_RoadBuilt_Advanced" };

        internal static void ApplyThoughtToWaymakers(int tier, int mood, int durationDays)
        {
            var meme = WaymakersMod.Meme;
            if (meme == null) return;

            var thoughtDef = DefDatabase<ThoughtDef>.GetNamed(TierDefNames[tier]);
            if (thoughtDef == null) return;

            int baseMood = new[] { 2, 5, 9, 13 }[tier];
            int stageIndex = mood - baseMood;
            if (stageIndex < 0) stageIndex = 0;
            if (stageIndex >= thoughtDef.stages.Count) stageIndex = thoughtDef.stages.Count - 1;

            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (pawn?.Ideo?.HasMeme(meme) == true)
                {
                    var memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
                    memory.SetForcedStage(stageIndex);
                    memory.durationTicksOverride = durationDays * 60000;
                    pawn.needs.mood?.thoughts?.memories?.TryGainMemory(memory);
                }
            }
        }
    }

    public static class Patch_FinaliseConstructionSite
    {
        public static void Postfix(object site)
        {
            try
            {
                int count = CountLegs(site);
                LegCountTracker.Store(site, count);

                _ = Traverse.Create(site).Field("roadDef").GetValue() as RoadDef;
            }
            catch (Exception e)
            {
                Log.Error($"[Waymakers] Error in FinaliseConstructionSite postfix: {e}");
            }
        }

        private static int CountLegs(object site)
        {
            int count = 0;
            object leg = Traverse.Create(site).Field("LastLeg").GetValue();

            while (leg != null && leg != site)
            {
                count++;
                var legType = leg.GetType();
                if (legType.Name == "RoadConstructionLeg")
                    leg = Traverse.Create(leg).Field("previous").GetValue();
                else
                    break;
            }

            return count;
        }
    }

    public static class LegCountTracker
    {
        private static Dictionary<int, int> storage = new Dictionary<int, int>();

        public static void Store(object site, int count)
        {
            int id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(site);
            storage[id] = count;
        }

        public static int Pop(object site)
        {
            int id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(site);
            if (storage.TryGetValue(id, out int count))
            {
                storage.Remove(id);
                return count;
            }
            return 1;
        }
    }

    public static class Patch_CaravanWork
    {
        public static void Postfix(object __instance, ref float __result)
        {
            if (WaymakersMod.CoordinateWorksHediff == null) return;

            var caravan = Traverse.Create(__instance).Field("parent").GetValue() as Caravan;
            if (caravan == null) return;

            if (CaravanUtils.HasCoordinateWorksBuff(caravan))
                __result *= 2f;
        }
    }

    public static class Patch_RoadSiteInspect
    {
        public static void Postfix(object __instance, ref string __result)
        {
            if (WaymakersMod.CoordinateWorksHediff == null) return;

            if (__instance is not WorldObject site) return;

            foreach (var obj in Find.WorldObjects.ObjectsAt(site.Tile))
            {
                if (obj is Caravan caravan && caravan.IsPlayerControlled && CaravanUtils.HasCoordinateWorksBuff(caravan))
                {
                    __result += "\n" + "WM_CoordinateWorksBuffActive".Translate();
                    return;
                }
            }
        }
    }
}
