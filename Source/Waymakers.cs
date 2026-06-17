using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Waymakers
{
    [StaticConstructorOnStartup]
    public static class WaymakersMod
    {
        public const string Id = "waymakers.meme";

        private static MemeDef memeDef;
        private static HediffDef coordinateWorksHediff;
        private static HediffDef groundbreakingBuffDef;
        private static HediffDef openingBuffDef;
        internal static int lastRoadBuildTick = -1;

        public static MemeDef Meme => memeDef;
        public static HediffDef CoordinateWorksHediff => coordinateWorksHediff;
        public static HediffDef GroundbreakingBuffDef => groundbreakingBuffDef;
        public static HediffDef OpeningBuffDef => openingBuffDef;

        static WaymakersMod()
        {
            var harmony = new Harmony(Id);

            try
            {
                var endMethod = AccessTools.Method("RailsAndRoadsOfTheRim.WorldObjectComp_ConstructionSite:EndConstruction");
                if (endMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(Patch_EndConstruction), nameof(Patch_EndConstruction.Prefix));
                    harmony.Patch(endMethod, prefix: prefix);
                    Log.Message("[Waymakers] Patched EndConstruction.");
                }
                else Log.Error("[Waymakers] EndConstruction not found.");

                var finaliseMethod = AccessTools.Method("RailsAndRoadsOfTheRim.RailsAndRoadsOfTheRim:FinaliseConstructionSite");
                if (finaliseMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_FinaliseConstructionSite), nameof(Patch_FinaliseConstructionSite.Postfix));
                    harmony.Patch(finaliseMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched FinaliseConstructionSite.");
                }
                else Log.Error("[Waymakers] FinaliseConstructionSite not found.");

                var workMethod = AccessTools.Method("RailsAndRoadsOfTheRim.WorldObjectComp_Caravan:AmountOfWork");
                if (workMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_CaravanWork), nameof(Patch_CaravanWork.Postfix));
                    harmony.Patch(workMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched AmountOfWork (caravan work boost).");
                }
                else Log.Error("[Waymakers] AmountOfWork not found.");
            }
            catch (Exception e)
            {
                Log.Error($"[Waymakers] Patching failed: {e}");
            }

            memeDef = DefDatabase<MemeDef>.GetNamed("WM_Waymakers");
            coordinateWorksHediff = DefDatabase<HediffDef>.GetNamed("WM_CoordinateWorks");
            groundbreakingBuffDef = DefDatabase<HediffDef>.GetNamed("WM_GroundbreakingBuff");
            openingBuffDef = DefDatabase<HediffDef>.GetNamed("WM_OpeningBuff");

            if (memeDef == null)
                Log.Error("[Waymakers] MemeDef 'WM_Waymakers' not found.");

            Log.Message("[Waymakers] Loaded.");
        }
    }

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

                Log.Message($"[Waymakers] Road completed: {roadDef.label}, legs={legs}, qualityTier={qualityTier}, mood=+{mood}, duration={durationDays}d");

                WaymakersMod.lastRoadBuildTick = Find.TickManager.TicksGame;

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

        private static void ApplyThoughtToWaymakers(int tier, int mood, int durationDays)
        {
            var meme = WaymakersMod.Meme;
            if (meme == null) return;

            var thoughtDef = DefDatabase<ThoughtDef>.GetNamed(TierDefNames[tier]);
            if (thoughtDef == null) return;

            int baseMood = new[] { 2, 5, 9, 13 }[tier];
            int stageIndex = mood - baseMood;
            if (stageIndex < 0) stageIndex = 0;
            if (stageIndex >= thoughtDef.stages.Count) stageIndex = thoughtDef.stages.Count - 1;

            int maxDuration = (int)thoughtDef.durationDays;
            if (durationDays > maxDuration) durationDays = maxDuration;

            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (pawn?.Ideo?.HasMeme(meme) == true)
                {
                    var memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
                    memory.SetForcedStage(stageIndex);
                    int ageOffsetTicks = (maxDuration - durationDays) * 60000;
                    if (ageOffsetTicks > 0)
                        Traverse.Create(memory).Field("age").SetValue(ageOffsetTicks);
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

                var roadDef = Traverse.Create(site).Field("roadDef").GetValue() as RoadDef;
                // Log.Message($"[Waymakers] Construction started: road={roadDef?.label}, legs={count}");
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

            foreach (var pawn in caravan.PawnsListForReading)
            {
                if (pawn.health?.hediffSet?.HasHediff(WaymakersMod.CoordinateWorksHediff) == true)
                {
                    __result *= 1.75f;
                    return;
                }
            }
        }
    }

    public class ThoughtWorker_Precept_RecentRoadBuild : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
    {
        private static readonly int ThresholdDays = 30;

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!p.IsColonist || p.IsSlave)
                return false;

            int lastTick = WaymakersMod.lastRoadBuildTick;
            // Use settleTick as baseline for new colonies so they get a 30-day grace period
            if (lastTick < 0)
                lastTick = Find.TickManager.SettleTick;

            int daysSince = (Find.TickManager.TicksGame - lastTick) / 60000;
            return ThoughtState.ActiveAtStage(daysSince > ThresholdDays ? 1 : 0);
        }

        public IEnumerable<NamedArgument> GetDescriptionArgs()
        {
            yield return ThresholdDays.Named("DAYSSINCELASTROADTHRESHOLD");
        }
    }

    public class RitualOutcomeEffectWorker_WMGroundbreaking : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_WMGroundbreaking() { }
        public RitualOutcomeEffectWorker_WMGroundbreaking(RitualOutcomeEffectDef def) : base(def) { }

        public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            base.Apply(progress, totalPresence, jobRitual);
            float quality = GetQuality(jobRitual, progress);
            var hediff = WaymakersMod.GroundbreakingBuffDef;
            if (hediff == null || quality <= 0.25f) return;
            foreach (var kvp in totalPresence)
            {
                // Hediff added to brain for UI visibility; ticksToDisappear set manually
                // (aura hediffs get this from GiveHediffsInRange, but ritual hediffs don't)
                var h = kvp.Key.health.AddHediff(hediff, kvp.Key.health.hediffSet.GetBrain());
                h.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000;
            }
        }
    }

    public class RitualOutcomeEffectWorker_WMOpening : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_WMOpening() { }
        public RitualOutcomeEffectWorker_WMOpening(RitualOutcomeEffectDef def) : base(def) { }

        public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            base.Apply(progress, totalPresence, jobRitual);
            float quality = GetQuality(jobRitual, progress);
            var hediff = WaymakersMod.OpeningBuffDef;
            if (hediff == null || quality <= 0.25f) return;
            foreach (var kvp in totalPresence)
            {
                var h = kvp.Key.health.AddHediff(hediff, kvp.Key.health.hediffSet.GetBrain());
                h.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000;
            }
        }
    }
}
