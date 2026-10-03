using RimWorld;
using Verse;

namespace KensMod
{
    [DefOf]
    public static class KM_DefOf
    {
        public static ThingCategoryDef KM_Teas;

        public static JobDef KM_DrinkFromTeapot;
        public static JobDef KM_FillTeapot;

        public static ThoughtDef KM_SharedTea;
        public static ThoughtDef KM_SharedTeaSocial;

        public static HediffDef KM_RecentTea;

        static KM_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(KM_DefOf));
        }
    }
}
