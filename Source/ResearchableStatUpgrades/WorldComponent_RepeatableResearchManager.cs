using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ResearchableStatUpgrades
{
    public class WorldComponent_RepeatableResearchManager : WorldComponent
    {
        public Dictionary<ResearchProjectDef, int> researchedFactor = new Dictionary<ResearchProjectDef, int>();

        public WorldComponent_RepeatableResearchManager(World world) : base(world)
        {
            foreach (var d in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (d.GetModExtension<ModExtension_ResearchScaleable>() != null)
                {
                    researchedFactor.Add(d, 0);
                }
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            for (int i = 0; i < researchedFactor.Keys.Count; i++)
            {
                var def = researchedFactor.Keys.ElementAt(i);
                ModifyNewDef(def);
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref researchedFactor, "researchedTimes", LookMode.Def, LookMode.Value);

        }

        /// <summary>
        /// Currently modifies the def's decription.
        /// </summary>
        public void ModifyNewDef(ResearchProjectDef def)
        {
            //Remove old number
            int i = def.description.Length;
            for (; i >= 0; i--)
            {
                var character = def.description[i - 1];
                if (!int.TryParse(character.ToString(), out int f))
                    break;
            }
            if (i != def.description.Length)
                def.description = def.description.Remove(i);
            //Add new number
            def.description += researchedFactor[def].ToString();
            //Resolve cost
            def.GetModExtension<ModExtension_ResearchScaleable>().ResolveCost(ref def, researchedFactor[def]);
        }

        public void AddToDictionary(ResearchProjectDef def, int factor = 1)
        {
            if (researchedFactor.ContainsKey(def))
            {
                researchedFactor[def] += factor;
            }
            else
            {
                researchedFactor.Add(def, factor);
            }
            ModifyNewDef(def);
        }

        public int GetFactorFor(ResearchProjectDef def)
        {
            if (def == null || !researchedFactor.ContainsKey(def))
                return 0;
            return researchedFactor[def];
        }
    }
}
