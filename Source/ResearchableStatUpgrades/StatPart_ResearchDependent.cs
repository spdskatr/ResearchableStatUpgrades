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
            float factorBase = 1f;
            TransformValue(req, ref factorBase);
            string str = factorBase.ToStringPercent();
            return str == "100%" ? string.Empty : "RSU_FactorFromResearch".Translate() + str;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.def.race != null)
            {
                for (int i = 0; i < researchFactors.Count; i++)
                {
                    var factor = researchFactors[i];
                    if (ShouldApplyFactorToRequest(req, factor))
                    {
                        val *= factor.factor;
                    }
                }
                for (int i = 0; i < repeatables.Count; i++)
                {
                    var repeatable = repeatables[i];
                    if (ShouldApplyFactorToRequest(req, repeatable))
                    {
                        var rpts = RSUUtil.RepeatableResearchManager.GetFactorFor(repeatable.def);
                        for (i = 0; i < rpts; i++)
                        {
                            val *= repeatable.factor;
                        }
                    }
                }
            }
        }

        private static bool ShouldApplyFactorToRequest(StatRequest req, ResearchFactor factor)
        {
            return (factor.def.IsFinished || factor.def.IsRepeatableResearch()) && req.HasThing &&
                (req.Thing.def.race.Humanlike || factor.applyToNonHumanlike) && 
                (req.Thing.Faction == Faction.OfPlayer || factor.applyToNonColonistFaction);
        }
    }
    public class ResearchFactor
    {
        public ResearchProjectDef def;
        public float factor = 1f;
        public bool applyToNonColonistFaction = false;
        public bool applyToNonHumanlike = false;
    }
}
