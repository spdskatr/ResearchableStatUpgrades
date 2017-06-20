using RimWorld;
using System;
using Verse;

namespace ResearchableStatUpgrades
{
    [DefOf]
    public static class RSUUtil
    {
        public static ThoughtDef AteAwfulMealFlavored;

        public static WorldComponent_DefEditingResearchManager DefEditingResearchManager => Find.World.GetComponent<WorldComponent_DefEditingResearchManager>();

        public static WorldComponent_RepeatableResearchManager RepeatableResearchManager => Find.World.GetComponent<WorldComponent_RepeatableResearchManager>();

        public static WorldComponent_StackCountEditManager StackCountEditManager => Find.World.GetComponent<WorldComponent_StackCountEditManager>();

        public static bool IsInst(this Type t, Type a) => t.IsSubclassOf(a) || t == a;

        public static bool IsSingleStackWeapon(this ThingDef t) => WorldComponent_StackCountEditManager.originalStackCounts[t] == 1 && t.Verbs.Any();
    }
}
