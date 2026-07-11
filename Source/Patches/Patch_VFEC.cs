using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Waymakers
{
    [StaticConstructorOnStartup]
    public static class Patch_VFEC_Tick
    {
        private static readonly bool _vfecPresent =
            ModLister.GetActiveModWithIdentifier("OskarPotocki.VFE.Classical") != null;
        private static readonly Dictionary<int, int> prevWorkDone = new();

        public static void Prefix(WorldObject __instance)
        {
            if (!_vfecPresent || WaymakersMod.CoordinateWorksHediff == null) return;
            if (__instance is not Caravan caravan) return;
            if (!caravan.IsHashIntervalTick(250)) return;
            ApplyBuff(caravan);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        // All VFEC type references live in ApplyBuff, not in Prefix.
        // [MethodImpl(NoInlining)] prevents the JIT from compiling ApplyBuff
        // until it is actually called. Since Prefix returns early when VFEC
        // is absent, ApplyBuff is never compiled and VFEC types are never
        // resolved - avoiding TypeLoadException at startup.
        private static void ApplyBuff(Caravan caravan)
        {
            int id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(caravan);

            var inst = VFEC.WorldComponent_RoadBuilding.Instance;
            if (inst == null) return;
            if (!inst.WorkInfos.TryGetValue(caravan, out var wi))
            {
                prevWorkDone.Remove(id);
                return;
            }
            if (wi.WorkDone >= wi.WorkTotal)
            {
                prevWorkDone.Remove(id);
                return;
            }

            if (prevWorkDone.TryGetValue(id, out var prev))
            {
                int delta = wi.WorkDone - prev;
                if (delta > 0 && CaravanUtils.HasCoordinateWorksBuff(caravan))
                {
                    wi.WorkDone += delta;
                    if (Prefs.DevMode)
                        Log.Message($"[Waymakers] VFEC buff: +{delta} on tile {caravan.Tile}");
                }
            }
            prevWorkDone[id] = wi.WorkDone;
        }
    }
}
