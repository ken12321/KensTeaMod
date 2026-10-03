using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace KensMod
{
    // A = teapot, B = tea to pour in.
    public class JobDriver_FillTeapot : JobDriver
    {
        private const int FillTicks = 90;

        private Building_Teapot Teapot => (Building_Teapot)job.GetTarget(TargetIndex.A).Thing;

        private Thing Tea => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Teapot, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(Tea, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => Teapot.TeaDef == null || Teapot.TeaDef != job.GetTarget(TargetIndex.B).Thing?.def);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(FillTicks)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .WithProgressBarToilDelay(TargetIndex.A);

            Toil fill = ToilMaker.MakeToil("FillTeapot");
            fill.initAction = delegate
            {
                Thing carried = pawn.carryTracker.CarriedThing;
                Teapot.FillFrom(pawn.carryTracker.GetDirectlyHeldThings(), carried);
                if (pawn.carryTracker.CarriedThing != null)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
                }
            };
            fill.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return fill;
        }
    }
}
