using RimWorld.Planet;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using RimWorld;

namespace ResearchableStatUpgrades
{
    /// <summary>
    /// A different research manager focused on one def, allowing dynamic stackable researches
    /// </summary>
    public class WorldComponent_StackCountEditManager : WorldComponent
    {
        public static readonly Dictionary<ThingDef, int> originalStackCounts;
        public static HashSet<ThingDef> prostheses;
        static readonly IEnumerable<ResearchProjectDef> modExtensionResearches;
        float baseFactor = 1f;

        public float CurFactor
        {
            get => baseFactor;
            set => baseFactor = value;
        }
        public WorldComponent_StackCountEditManager(World world) : base(world)
        {
        }
        static WorldComponent_StackCountEditManager()
        {
            originalStackCounts = new Dictionary<ThingDef, int>();
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                originalStackCounts.Add(def, def.stackLimit);
            }
            modExtensionResearches = from r in DefDatabase<ResearchProjectDef>.AllDefs
                                     where r.HasModExtension<ModExtension_StackCountFactor>()
                                     select r;
            // Prostheses
            prostheses = new HashSet<ThingDef>(from RecipeDef r in DefDatabase<RecipeDef>.AllDefs
                                               where r.workerClass.IsInst(typeof(Recipe_Surgery))
                                               from IngredientCount i in r.ingredients
                                               where i.GetBaseCount() == 1f
                                               from ThingDef def in i.filter.AllowedThingDefs
                                               where originalStackCounts[def] == 1
                                               select def); // Repeats are automatically eliminated by hash set
        }
        ~WorldComponent_StackCountEditManager()
        {
            //reset
            foreach (var pair in originalStackCounts)
            {
                pair.Key.stackLimit = pair.Value;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseFactor, "curFactor", 1f);

            //Called just after defs loaded
            //Note: WorldComponent.FinalizeInit is indeed called BEFORE spawning of things, but ResolveCrossRefs undoes any def changes
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                RefreshStackCountEdits();
            }
        }

        public void MultiplyFactorBy(float by)
        {
            CurFactor *= by;
            RefreshStackCountEdits();
        }
        
        public void RefreshResearches()
        {
            CurFactor = 1f;
            foreach (var e in modExtensionResearches)
            {
                if (e.IsFinished || RSUUtil.RepeatableResearchManager.researchedFactor.ContainsKey(e))
                {
                    var modExtension = e.GetModExtension<ModExtension_StackCountFactor>();
                    modExtension.ApplyWorker(e);
                }
            }
        }

        private void RefreshStackCountEdits()
        {
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                try
                {
                    if (tDef.thingClass.IsInst(typeof(Corpse), typeof(Apparel), typeof(MinifiedThing), typeof(UnfinishedThing)) 
                        || tDef.IsSingleStackWeapon() 
                        || tDef.category != ThingCategory.Item
                        || prostheses.Contains(tDef))
                        continue;
                    int newLimit = Mathf.FloorToInt(originalStackCounts[tDef] * CurFactor);
                    //For freak situations when an overflow occurs
                    if (newLimit < 0)
                    {
                        newLimit = int.MaxValue;
                    }
                    if (newLimit > 1)
                    {
                        //Just in case their thing labels are disabled, re-enable
                        tDef.drawGUIOverlay = true;
                    }

                    tDef.stackLimit = newLimit;
                }
                catch (Exception ex)
                {
                    Log.Error("Exception editing stack counts for ThingDef \"" + tDef.defName + "\": " + ex);
                }
            }
        }
    }

}
