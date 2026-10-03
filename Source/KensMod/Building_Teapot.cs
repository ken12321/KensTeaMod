using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace KensMod
{
    public class Building_Teapot : Building, IThingHolder
    {
        private static readonly TeapotExtension DefaultExtension = new TeapotExtension();

        private ThingOwner<Thing> innerContainer;
        private ThingDef teaDef;

        public Building_Teapot()
        {
            innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
        }

        public TeapotExtension Ext => def.GetModExtension<TeapotExtension>() ?? DefaultExtension;

        public ThingDef TeaDef => teaDef;

        public int TeaCount => innerContainer.TotalStackCount;

        public int SpaceLeft => teaDef == null ? 0 : Mathf.Max(0, Ext.capacity - TeaCount);

        public bool WantsRefill => teaDef != null && SpaceLeft >= Mathf.Min(Ext.refillWhenMissing, Ext.capacity);

        public Thing PeekCup => innerContainer.Count > 0 ? innerContainer[0] : null;

        public static bool IsTea(ThingDef d)
        {
            return d.thingCategories != null && d.thingCategories.Contains(KM_DefOf.KM_Teas);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Defs.Look(ref teaDef, "teaDef");
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        // Moves as much of the carried stack into the pot as fits. Returns the number added.
        public int FillFrom(ThingOwner source, Thing tea)
        {
            if (tea == null || tea.def != teaDef)
            {
                return 0;
            }
            int count = Mathf.Min(SpaceLeft, tea.stackCount);
            if (count <= 0)
            {
                return 0;
            }
            return source.TryTransferToContainer(tea, innerContainer, count);
        }

        public Thing TakeOneCup()
        {
            Thing cup = PeekCup;
            return cup == null ? null : innerContainer.Take(cup, 1);
        }

        public void SetTeaDef(ThingDef newDef)
        {
            if (newDef == teaDef)
            {
                return;
            }
            EjectContents();
            teaDef = newDef;
        }

        private void EjectContents()
        {
            if (Spawned)
            {
                innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            // Minifying despawns with Vanish and keeps the tea inside; deconstructing or destruction spills it.
            if (mode != DestroyMode.Vanish)
            {
                EjectContents();
            }
            base.DeSpawn(mode);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction != Faction.OfPlayer)
            {
                yield break;
            }
            yield return new Command_Action
            {
                defaultLabel = teaDef == null ? "KM_TeapotNoTea".Translate().ToString() : teaDef.LabelCap.ToString(),
                defaultDesc = "KM_TeapotSelectDesc".Translate(Ext.capacity),
                icon = teaDef != null ? teaDef.uiIcon : TexCommand.ClearPrioritizedWork,
                action = OpenTeaMenu
            };
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }
            string label = teaDef == null
                ? "KM_DrinkFromTeapotGeneric".Translate().ToString()
                : "KM_DrinkFromTeapot".Translate(teaDef.label).ToString();
            if (!selPawn.CanReach(this, PathEndMode.Touch, Danger.Deadly))
            {
                yield return new FloatMenuOption(label + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            string reason = JoyGiver_Teapot.CannotDrinkReason(selPawn, this);
            if (reason != null)
            {
                yield return new FloatMenuOption(label + ": " + reason, null);
                yield break;
            }
            // Ordering past the drug policy is allowed, but flag it: it's why pawns won't drink here on their own.
            if (JoyGiver_Teapot.DrugPolicyForbidsJoy(selPawn, teaDef))
            {
                label += " " + "KM_DrinkFromTeapotPolicyNote".Translate();
            }
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
            {
                Job job = JobMaker.MakeJob(KM_DefOf.KM_DrinkFromTeapot, this);
                job.playerForced = true;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), selPawn, this);
        }

        private void OpenTeaMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("KM_TeapotNoTea".Translate(), () => SetTeaDef(null))
            };
            foreach (ThingDef d in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (IsTea(d))
                {
                    ThingDef local = d;
                    options.Add(new FloatMenuOption(d.LabelCap, () => SetTeaDef(local), d));
                }
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            if (teaDef == null)
            {
                sb.Append("KM_TeapotNoTeaInspect".Translate());
            }
            else
            {
                sb.Append("KM_TeapotContents".Translate(TeaCount, Ext.capacity, teaDef.label));
            }
            return sb.ToString();
        }
    }
}
