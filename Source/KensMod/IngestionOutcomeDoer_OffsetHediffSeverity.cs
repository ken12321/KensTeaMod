using RimWorld;
using Verse;

namespace KensMod
{
    // Changes the severity of an existing hediff, e.g. a negative offset to shorten food poisoning.
    public class IngestionOutcomeDoer_OffsetHediffSeverity : IngestionOutcomeDoer
    {
        public HediffDef hediffDef;
        public float offset;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
            {
                hediff.Severity += offset * ingestedCount;
            }
        }
    }
}
