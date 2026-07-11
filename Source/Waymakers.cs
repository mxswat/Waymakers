using System;
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
        private static AbilityDef coordinateWorksAbilityDef;
        public static MemeDef Meme => memeDef;
        public static HediffDef CoordinateWorksHediff => coordinateWorksHediff;
        public static HediffDef GroundbreakingBuffDef => groundbreakingBuffDef;
        public static AbilityDef CoordinateWorksAbilityDef => coordinateWorksAbilityDef;

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

                var roadInspectMethod = AccessTools.Method("RailsAndRoadsOfTheRim.RoadConstructionSite:GetInspectString");
                if (roadInspectMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_RoadSiteInspect), nameof(Patch_RoadSiteInspect.Postfix));
                    harmony.Patch(roadInspectMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched RoadConstructionSite.GetInspectString.");
                }
                else Log.Message("[Waymakers] RotR not loaded, RoadConstructionSite patch skipped.");

                var overlayRoadMethod = AccessTools.Method(typeof(WorldGrid), "OverlayRoad", new[] { typeof(PlanetTile), typeof(PlanetTile), typeof(RoadDef) });
                if (overlayRoadMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Patch_OverlayRoad), nameof(Patch_OverlayRoad.Postfix));
                    harmony.Patch(overlayRoadMethod, postfix: postfix);
                    Log.Message("[Waymakers] Patched WorldGrid.OverlayRoad.");
                }
                else Log.Error("[Waymakers] WorldGrid.OverlayRoad not found.");

                if (ModLister.GetActiveModWithIdentifier("OskarPotocki.VFE.Classical") != null)
                    Log.Message("[Waymakers] VFEC detected, road work buff.");

                var tickMethod = AccessTools.Method(typeof(WorldObject), "Tick");
                if (tickMethod != null)
                {
                    harmony.Patch(tickMethod,
                        prefix: new HarmonyMethod(typeof(Patch_VFEC_Tick), nameof(Patch_VFEC_Tick.Prefix)));
                    Log.Message("[Waymakers] Patched WorldObject.Tick (VFEC buff).");
                }
            }
            catch (Exception e)
            {
                Log.Error($"[Waymakers] Patching failed: {e}");
            }

            memeDef = DefDatabase<MemeDef>.GetNamed("WM_Waymakers");
            coordinateWorksHediff = DefDatabase<HediffDef>.GetNamed("WM_CoordinateWorks");
            groundbreakingBuffDef = DefDatabase<HediffDef>.GetNamed("WM_GroundbreakingBuff");
            coordinateWorksAbilityDef = DefDatabase<AbilityDef>.GetNamed("WM_CoordinateWorks");

            if (memeDef == null)
                Log.Error("[Waymakers] MemeDef 'WM_Waymakers' not found.");

            Log.Message("[Waymakers] Loaded.");
        }
    }
}
