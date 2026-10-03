using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace KensMod
{
    // A = teapot, B = the cup of tea once poured.
    public class JobDriver_DrinkFromTeapot : JobDriver
    {
        private const int PourTicks = 60;
        private const float SharedTeaRadius = 7f;

        private Building_Teapot Teapot => (Building_Teapot)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // No reservation: several pawns can gather around one teapot.
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil pour = ToilMaker.MakeToil("PourTea");
            pour.initAction = delegate
            {
                Thing cup = Teapot.TakeOneCup();
                if (cup == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                if (!pawn.carryTracker.TryStartCarry(cup))
                {
                    GenPlace.TryPlaceThing(cup, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                job.SetTarget(TargetIndex.B, pawn.carryTracker.CarriedThing);
            };
            pour.defaultCompleteMode = ToilCompleteMode.Delay;
            pour.defaultDuration = PourTicks;
            pour.WithProgressBarToilDelay(TargetIndex.A);
            yield return pour;

            yield return Toils_Ingest.ChewIngestible(pawn, 1f, TargetIndex.B, TargetIndex.A);
            yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.B);

            Toil shared = ToilMaker.MakeToil("SharedTea");
            shared.initAction = GiveSharedTeaThoughts;
            shared.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return shared;
        }

        private void GiveSharedTeaThoughts()
        {
            if (pawn.needs?.mood == null)
            {
                return;
            }
            Room room = pawn.GetRoom();
            foreach (Pawn other in pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (other == pawn || other.CurJobDef != KM_DefOf.KM_DrinkFromTeapot || other.needs?.mood == null)
                {
                    continue;
                }
                if (!other.Position.InHorDistOf(pawn.Position, SharedTeaRadius) || other.GetRoom() != room)
                {
                    continue;
                }
                pawn.needs.mood.thoughts.memories.TryGainMemory(KM_DefOf.KM_SharedTea);
                other.needs.mood.thoughts.memories.TryGainMemory(KM_DefOf.KM_SharedTea);
                pawn.needs.mood.thoughts.memories.TryGainMemory(KM_DefOf.KM_SharedTeaSocial, other);
                other.needs.mood.thoughts.memories.TryGainMemory(KM_DefOf.KM_SharedTeaSocial, pawn);
            }
        }
    }
}
