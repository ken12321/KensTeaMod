using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace KensMod
{
    public class JoyGiver_Teapot : JoyGiver
    {
        private static readonly List<Thing> tmpCandidates = new List<Thing>();

        public override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveJobInternal(pawn, null);
        }

        public override Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatheringSpot, float maxRadius = -1f)
        {
            return TryGiveJobInternal(pawn, t => GatheringsUtility.InGatheringArea(t.Position, gatheringSpot, pawn.Map)
                && (maxRadius < 0f || t.Position.InHorDistOf(gatheringSpot, maxRadius)));
        }

        private Job TryGiveJobInternal(Pawn pawn, Predicate<Thing> extraValidator)
        {
            tmpCandidates.Clear();
            GetSearchSet(pawn, tmpCandidates);
            if (tmpCandidates.Count == 0)
            {
                return null;
            }
            Thing pot = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, tmpCandidates, PathEndMode.Touch,
                TraverseParms.For(pawn), 9999f, t => CanDrinkFrom(pawn, t) && (extraValidator == null || extraValidator(t)));
            tmpCandidates.Clear();
            return pot == null ? null : JobMaker.MakeJob(def.jobDef, pot);
        }

        public static bool CanDrinkFrom(Pawn pawn, Thing t)
        {
            if (!(t is Building_Teapot pot))
            {
                return false;
            }
            Thing cup = pot.PeekCup;
            if (cup == null || !pawn.WillEat(cup))
            {
                return false;
            }
            if (t.IsForbidden(pawn) || t.Fogged() || t.IsBurning() || !t.IsSociallyProper(pawn) || !t.IsPoliticallyProper(pawn))
            {
                return false;
            }
            // Same drug policy rule as vanilla recreational drug use.
            if (cup.def.IsDrug && pawn.drugs != null && !pawn.drugs.CurrentPolicy[cup.def].allowedForJoy
                && pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) <= 0 && !pawn.InMentalState)
            {
                return false;
            }
            return true;
        }
    }
}
