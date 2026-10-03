using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KensMod
{
    public class CompDryable : ThingComp
    {
        private float dryProgress;

        public CompProperties_Dryable Props => (CompProperties_Dryable)props;

        public float DryProgress => dryProgress;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref dryProgress, "dryProgress", 0f);
        }

        public override void CompTickInterval(int delta)
        {
            TickInterval(delta);
        }

        public override void CompTickRare()
        {
            TickInterval(250);
        }

        private void TickInterval(int delta)
        {
            if (!parent.Spawned)
            {
                return;
            }
            float factor = CurrentSpeedFactor();
            if (factor <= 0f || PausedByRain())
            {
                return;
            }
            dryProgress += delta / (GenDate.TicksPerDay * Props.daysToDry) * factor;
            if (dryProgress >= 1f)
            {
                FinishDrying();
            }
        }

        public float CurrentSpeedFactor()
        {
            Map map = parent.Map;
            IntVec3 cell = parent.Position;
            List<Thing> things = map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < things.Count; i++)
            {
                DryingSurfaceExtension ext = things[i].def.GetModExtension<DryingSurfaceExtension>();
                if (ext != null)
                {
                    return ext.speedFactor;
                }
            }
            return 0f;
        }

        private bool PausedByRain()
        {
            Map map = parent.Map;
            return map.weatherManager.RainRate > 0f && !parent.Position.Roofed(map);
        }

        private void FinishDrying()
        {
            Map map = parent.Map;
            IntVec3 cell = parent.Position;
            int count = parent.stackCount;
            bool forbidden = parent.IsForbidden(Faction.OfPlayer);

            Thing dried = ThingMaker.MakeThing(Props.driedThingDef);
            dried.stackCount = count;
            parent.Destroy();
            GenPlace.TryPlaceThing(dried, cell, map, ThingPlaceMode.Near);
            if (forbidden)
            {
                dried.SetForbidden(true, warnOnFail: false);
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (parent.stackCount + count);
            float other = ((ThingWithComps)otherStack).GetComp<CompDryable>().dryProgress;
            dryProgress = Mathf.Lerp(dryProgress, other, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CompDryable>().dryProgress = dryProgress;
        }

        public override string CompInspectStringExtra()
        {
            if (!parent.Spawned)
            {
                return null;
            }
            string pct = dryProgress.ToStringPercent();
            if (CurrentSpeedFactor() <= 0f)
            {
                return "KM_NotDrying".Translate(pct);
            }
            if (PausedByRain())
            {
                return "KM_DryingPausedRain".Translate(pct);
            }
            return "KM_Drying".Translate(pct);
        }
    }
}
