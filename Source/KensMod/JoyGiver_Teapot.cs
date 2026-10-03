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
            return t is Building_Teapot pot && !t.Fogged() && CannotDrinkReason(pawn, pot) == null
                && !DrugPolicyForbidsJoy(pawn, pot.PeekCup.def);
        }

        // Why the pawn can't pour a cup right now, or null if they can. Drug policy is separate, since players may override it.
        public static string CannotDrinkReason(Pawn pawn, Building_Teapot pot)
        {
            Thing cup = pot.PeekCup;
            if (cup == null)
            {
                return pot.TeaDef == null ? "KM_CannotDrinkNoTeaSelected".Translate() : "KM_CannotDrinkEmpty".Translate();
            }
            // Def overload on purpose: tea has no nutrition, so it's outside food policies, and the Thing overload
            // treats that as "not allowed". Drug policy governs tea instead.
            if (!pawn.WillEat(cup.def))
            {
                return "KM_CannotDrinkWontDrink".Translate(cup.def.label);
            }
            if (pot.IsForbidden(pawn))
            {
                return "KM_CannotDrinkForbidden".Translate();
            }
            if (pot.IsBurning())
            {
                return "KM_CannotDrinkBurning".Translate();
            }
            if (!pot.IsSociallyProper(pawn) || !pot.IsPoliticallyProper(pawn))
            {
                return "KM_CannotDrinkImproper".Translate();
            }
            return null;
        }

        // Same drug policy rule as vanilla recreational drug use.
        public static bool DrugPolicyForbidsJoy(Pawn pawn, ThingDef teaDef)
        {
            return teaDef.IsDrug && pawn.drugs != null && !pawn.drugs.CurrentPolicy[teaDef].allowedForJoy
                && pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) <= 0 && !pawn.InMentalState;
        }
    }
}
