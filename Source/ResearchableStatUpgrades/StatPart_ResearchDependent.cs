using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ResearchableStatUpgrades
{
    /// <summary>
    /// Alters colonist stats.
    /// </summary>
    public class StatPart_ResearchDependent : StatPart
    {
        public List<ResearchFactor> researchFactors = new List<ResearchFactor>();

        public List<ResearchFactor> repeatables = new List<ResearchFactor>();

        public void AddFactor(ResearchFactor factor)
        {
            researchFactors.Add(factor);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Faction == Faction.OfPlayer)
            {
                float factorBase = 1f;
                TransformValue(req, ref factorBase);
                return "RSU_FactorFromResearch".Translate() + factorBase.ToStringPercent();
            }
            return string.Empty;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.def.race != null && req.Thing.def.race.Humanlike && req.Thing.Faction == Faction.OfPlayer)
            {
                for (int i = 0; i < researchFactors.Count; i++)
                {
                    var factor = researchFactors[i];
                    if (factor.def.IsFinished)
                    {
                        val *= factor.factor;
                    }
                }
                for (int i = 0; i < repeatables.Count; i++)
                {
                    var repeatable = repeatables[i];
                    var rpts = Find.World.GetComponent<WorldComponent_RepeatableResearchManager>().GetFactorFor(repeatable.def);
                    for (i = 0; i < rpts; i++)
                    {
                        val *= repeatable.factor;
                    }
                }
            }
        }
    }
    public class ResearchFactor
    {
        public ResearchProjectDef def;
        public float factor;
    }
}
