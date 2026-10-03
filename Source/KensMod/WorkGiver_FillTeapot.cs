using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace KensMod
{
    public class WorkGiver_FillTeapot : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_Teapot pot) || pot.TeaDef == null || t.Faction != pawn.Faction)
            {
                return false;
            }
            if (pot.SpaceLeft <= 0 || (!forced && !pot.WantsRefill))
            {
                return false;
            }
            if (t.IsForbidden(pawn) || t.IsBurning() || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (FindTea(pawn, pot) == null)
            {
                JobFailReason.Is("KM_NoTeaToFill".Translate(pot.TeaDef.label));
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_Teapot pot = (Building_Teapot)t;
            Thing tea = FindTea(pawn, pot);
            if (tea == null)
            {
                return null;
            }
            Job job = JobMaker.MakeJob(KM_DefOf.KM_FillTeapot, pot, tea);
            job.count = Math.Min(pot.SpaceLeft, tea.stackCount);
            return job;
        }

        private static Thing FindTea(Pawn pawn, Building_Teapot pot)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(pot.TeaDef),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f,
                x => !x.IsForbidden(pawn) && pawn.CanReserve(x));
        }
    }
}
