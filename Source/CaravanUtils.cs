using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Waymakers
{
    public static class CaravanUtils
    {
        private static readonly System.Type VehiclePawnType = AccessTools.TypeByName("Vehicles.VehiclePawn");

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
            if (VehiclePawnType == null || !VehiclePawnType.IsInstanceOfType(pawn)) return;

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

        public static HashSet<Pawn> GetAllCaravanPawns(Caravan caravan)
        {
            var pawns = new HashSet<Pawn>();
            foreach (var p in caravan.PawnsListForReading)
            {
                if (!p.Dead && pawns.Add(p))
                    TryAddVehiclePassengers(p, pawns);
            }
            CollectAllPawnsRecursive(caravan, pawns);
            return pawns;
        }

        public static bool HasCoordinateWorksBuff(Caravan caravan)
        {
            foreach (var pawn in GetAllCaravanPawns(caravan))
                if (pawn.health?.hediffSet?.HasHediff(WaymakersMod.CoordinateWorksHediff) == true)
                    return true;
            return false;
        }
    }
}
