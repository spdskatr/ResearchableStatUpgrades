using RimWorld;
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
    }
}
