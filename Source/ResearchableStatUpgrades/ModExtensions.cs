using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ResearchableStatUpgrades
{
    public abstract class ModExtension_ResearchScaleable : DefModExtension
    {
        public abstract void ResolveCost(ref ResearchProjectDef def, int factor);
    }
    /// <summary>
    /// Mathematically speaking, cost(x) = bonus * x + baseCost;
    /// </summary>
    public class ModExtension_BaseBonus : ModExtension_ResearchScaleable
    {
        /// <summary>
        /// Cache for original base cost
        /// </summary>
        public float originalBaseCost = -1f;
        public float bonus = 0f;
        public override void ResolveCost(ref ResearchProjectDef def, int factor)
        {
            if (originalBaseCost < 0f)
                originalBaseCost = def.baseCost;
            def.baseCost = originalBaseCost + factor * bonus;
        }
    }
    /// <summary>
    /// Mathematically speaking, cost(x) = factor^x * baseCost;
    /// </summary>
    public class ModExtension_CompoundBonus : ModExtension_ResearchScaleable
    {
        /// <summary>
        /// Cache for original base cost
        /// </summary>
        public float originalBaseCost = -1f;
        public float factor = 1f;
        public override void ResolveCost(ref ResearchProjectDef def, int factor)
        {
            if (originalBaseCost < 0f)
                originalBaseCost = def.baseCost;
            def.baseCost = originalBaseCost * (Mathf.Pow(this.factor, factor));
        }
    }
}
