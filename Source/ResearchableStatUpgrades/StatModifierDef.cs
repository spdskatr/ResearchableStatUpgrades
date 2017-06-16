using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace ResearchableStatUpgrades
{
    /// <summary>
    /// Currently unused and unmaintained. Stats directly edited via <see cref="PatchOperation"/>s.
    /// </summary>
    public class StatModifierDef : Def
    {
        public StatDef def;

        public List<ResearchFactor> factors;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (def.parts == null)
                def.parts = new List<StatPart>();
            StatPart_ResearchDependent statPartResearchDependent = null;
            def.parts.OfType<StatPart_ResearchDependent>()?.TryRandomElement(out statPartResearchDependent);
            if (statPartResearchDependent == null)
            {
                statPartResearchDependent = new StatPart_ResearchDependent();
                for (int i = 0; i < factors.Count; i++)
                {
                    statPartResearchDependent.AddFactor(factors[i]);
                }
                def.parts.Add(statPartResearchDependent);
            }
        }
    }
}
