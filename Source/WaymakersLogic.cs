using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Waymakers
{
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

    public class ThoughtWorker_Precept_RecentRoadBuild : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
    {
        private static readonly int ThresholdDays = 30;

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!p.IsColonist || p.IsSlave)
                return false;

            var comp = Current.Game?.GetComponent<WaymakersGameComponent>();
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
}
