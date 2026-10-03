using RimWorld;
using Verse;

namespace KensMod
{
    // The ThoughtDef's requiredTraits limits this to Zen pawns; here we only check the tea timer.
    public class ThoughtWorker_ZenMissingTea : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return p.health?.hediffSet != null && !p.health.hediffSet.HasHediff(KM_DefOf.KM_RecentTea);
        }
    }
}
