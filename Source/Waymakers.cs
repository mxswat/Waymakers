using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
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
        public static MemeDef Meme => memeDef;
        public static HediffDef CoordinateWorksHediff => coordinateWorksHediff;
        public static HediffDef GroundbreakingBuffDef => groundbreakingBuffDef;

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

                var moteColorMethod = AccessTools.Method(typeof(HediffComp_GiveHediffsInRange), "CompPostTick");
                if (moteColorMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_MoteColor), nameof(Patch_MoteColor.Postfix));
                    harmony.Patch(moteColorMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched MoteColor (ideo tint).");
                }
                else Log.Error("[Waymakers] MoteColor method not found.");

                var caravanGizmoMethod = AccessTools.Method(typeof(Caravan), "GetGizmos");
                if (caravanGizmoMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_CaravanGizmos), nameof(Patch_CaravanGizmos.Postfix));
                    harmony.Patch(caravanGizmoMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched Caravan.GetGizmos (world pawns).");
                }
                else Log.Error("[Waymakers] Caravan.GetGizmos not found.");

                var inspectMethod = AccessTools.Method(typeof(Caravan), "GetInspectString");
                if (inspectMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_CaravanInspect), nameof(Patch_CaravanInspect.Postfix));
                    harmony.Patch(inspectMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched Caravan.GetInspectString.");
                }
                else Log.Error("[Waymakers] Caravan.GetInspectString not found.");
            }
            catch (Exception e)
            {
                Log.Error($"[Waymakers] Patching failed: {e}");
            }

            memeDef = DefDatabase<MemeDef>.GetNamed("WM_Waymakers");
            coordinateWorksHediff = DefDatabase<HediffDef>.GetNamed("WM_CoordinateWorks");
            groundbreakingBuffDef = DefDatabase<HediffDef>.GetNamed("WM_GroundbreakingBuff");

            if (memeDef == null)
                Log.Error("[Waymakers] MemeDef 'WM_Waymakers' not found.");

            Log.Message("[Waymakers] Loaded.");
        }
    }

    public class WaymakersGameComponent : GameComponent
    {
        public int LastRoadBuildTick = -1;
        public int MemeAdoptedTick = -1;

        public WaymakersGameComponent(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref LastRoadBuildTick, "waymakers_lastRoadBuildTick", -1);
            Scribe_Values.Look(ref MemeAdoptedTick, "waymakers_memeAdoptedTick", -1);
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
                if (qualityTier >= 2) durationDays *= 2;

                Log.Message($"[Waymakers] Road completed: {roadDef.label}, legs={legs}, qualityTier={qualityTier}, mood=+{mood}, duration={durationDays}d");

                var comp = Current.Game?.GetComponent<Waymakers.WaymakersGameComponent>();
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

            var pawns = new HashSet<Pawn>();
            foreach (var p in caravan.PawnsListForReading)
            {
                if (!p.Dead && pawns.Add(p))
                    Patch_CaravanGizmos.TryAddVehiclePassengers(p, pawns);
            }
            Patch_CaravanGizmos.CollectAllPawnsRecursive(caravan, pawns);

            foreach (var pawn in pawns)
            {
                if (pawn.health?.hediffSet?.HasHediff(WaymakersMod.CoordinateWorksHediff) == true)
                {
                    __result *= 2f;
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

            var comp = Current.Game?.GetComponent<Waymakers.WaymakersGameComponent>();
            if (comp == null)
                return false;

            if (comp.MemeAdoptedTick < 0 && p.Ideo?.HasMeme(WaymakersMod.Meme) == true)
                comp.MemeAdoptedTick = Find.TickManager.TicksGame;

            int baseline = Math.Max(comp.LastRoadBuildTick, comp.MemeAdoptedTick);

            if (baseline < 0)
                baseline = Find.TickManager.SettleTick;

            int daysSince = (Find.TickManager.TicksGame - baseline) / 60000;
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
            if (hediff == null || quality <= 0.5f) return;
            foreach (var kvp in totalPresence)
            {
                // Hediff added to brain for UI visibility; ticksToDisappear set manually
                // (aura hediffs get this from GiveHediffsInRange, but ritual hediffs don't)
                var h = kvp.Key.health.AddHediff(hediff, kvp.Key.health.hediffSet.GetBrain());
                h.Severity = quality;
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
            if (quality > 0.5f && jobRitual.Map != null && jobRitual.Map.IsPlayerHome)
            {
                var incident = quality > 0.7f
                    ? IncidentDefOf.TraderCaravanArrival
                    : IncidentDefOf.TravelerGroup;
                var parms = StorytellerUtility.DefaultParmsNow(incident.category, jobRitual.Map);
                incident.Worker.TryExecute(parms);
            }
        }
    }

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
            var abilityDef = DefDatabase<AbilityDef>.GetNamed("WM_CoordinateWorks");
            if (abilityDef == null) return null;

            var pawns = new HashSet<Pawn>();
            foreach (var p in caravan.PawnsListForReading)
            {
                if (!p.Dead && pawns.Add(p))
                    TryAddVehiclePassengers(p, pawns);
            }
            CollectAllPawnsRecursive(caravan, pawns);

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
                var def = DefDatabase<AbilityDef>.GetNamed("WM_CoordinateWorks");
                var allPawns = new HashSet<Pawn>();
                foreach (var p in caravan.PawnsListForReading)
                {
                    if (!p.Dead && allPawns.Add(p))
                        TryAddVehiclePassengers(p, allPawns);
                }
                CollectAllPawnsRecursive(caravan, allPawns);
                foreach (var p in allPawns)
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

        internal static void CollectAllPawnsRecursive(IThingHolder holder, HashSet<Pawn> result)
        {
            var children = new List<IThingHolder>();
            holder.GetChildHolders(children);
            foreach (var child in children)
            {
                if (child is ThingOwner thingOwner)
                {
                    foreach (var thing in thingOwner)
                    {
                        if (thing is Pawn p && !p.Dead && result.Add(p))
                        {
                            TryAddVehiclePassengers(p, result);
                            CollectAllPawnsRecursive(p, result);
                        }
                    }
                }
                CollectAllPawnsRecursive(child, result);
            }
        }

        internal static void TryAddVehiclePassengers(Pawn pawn, HashSet<Pawn> result)
        {
            var vehicleType = AccessTools.TypeByName("Vehicles.VehiclePawn");
            if (vehicleType == null || !vehicleType.IsInstanceOfType(pawn)) return;

            var passengers = Traverse.Create(pawn).Property("AllPawnsAboard").GetValue<IList<Pawn>>();
            if (passengers != null)
            {
                foreach (var passenger in passengers)
                {
                    if (!passenger.Dead)
                        result.Add(passenger);
                }
            }
        }
    }

    public static class Patch_CaravanInspect
    {
        public static void Postfix(Caravan __instance, ref string __result)
        {
            if (WaymakersMod.CoordinateWorksHediff == null) return;
            var pawns = new HashSet<Pawn>();
            foreach (var p in __instance.PawnsListForReading)
            {
                if (!p.Dead && pawns.Add(p))
                    Patch_CaravanGizmos.TryAddVehiclePassengers(p, pawns);
            }
            Patch_CaravanGizmos.CollectAllPawnsRecursive(__instance, pawns);

            foreach (var pawn in pawns)
            {
                if (pawn.health?.hediffSet?.HasHediff(WaymakersMod.CoordinateWorksHediff) == true)
                {
                    __result += "\nCoordination active (+100% road building speed)";
                    return;
                }
            }
        }
    }

    public static class Patch_MoteColor
    {
        public static void Postfix(HediffComp_GiveHediffsInRange __instance)
        {
            if (__instance.parent.def != WaymakersMod.CoordinateWorksHediff) return;
            var mote = Traverse.Create(__instance).Field("mote").GetValue<Mote>();
            if (mote != null && !mote.Destroyed)
                mote.instanceColor = __instance.parent.pawn.Ideo?.Color ?? Color.white;
        }
    }

}
