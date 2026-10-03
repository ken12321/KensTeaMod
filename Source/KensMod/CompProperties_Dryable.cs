using Verse;

namespace KensMod
{
    public class CompProperties_Dryable : CompProperties
    {
        // Thing this item turns into once fully dried.
        public ThingDef driedThingDef;

        // Days to dry at speed factor 1.0 (a drying rack).
        public float daysToDry = 2f;

        public CompProperties_Dryable()
        {
            compClass = typeof(CompDryable);
        }
    }
}
