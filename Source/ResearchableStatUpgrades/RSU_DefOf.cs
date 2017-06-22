using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ResearchableStatUpgrades
{
    [DefOf]
    public static class RSUDefOf
    {
        public static ThoughtDef AteAwfulMealFlavored;
    }
    public static class RSUUtil
    {
        public static readonly BindingFlags universal = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

        public static readonly FieldInfo researchModsField = typeof(ResearchProjectDef).GetField("researchMods", universal);

        public static WorldComponent_DefEditingResearchManager DefEditingResearchManager => Find.World.GetComponent<WorldComponent_DefEditingResearchManager>();

        public static WorldComponent_RepeatableResearchManager RepeatableResearchManager => Find.World.GetComponent<WorldComponent_RepeatableResearchManager>();

        public static WorldComponent_StackCountEditManager StackCountEditManager => Find.World.GetComponent<WorldComponent_StackCountEditManager>();

        public static bool IsInst(this Type t, Type a) => t.IsSubclassOf(a) || t == a;

        public static bool IsInst(this Type t, params Type[] types) => types.Any(t2 => IsInst(t, t2));

        public static bool IsSingleStackWeapon(this ThingDef t) => WorldComponent_StackCountEditManager.originalStackCounts[t] == 1 && t.Verbs.Any();

        public static ResearchMod GetResearchMod(this ResearchProjectDef def, Type type) => ((List<ResearchMod>)researchModsField.GetValue(def))?.Find(m => m.GetType().IsInst(type));

        public static T GetResearchMod<T>(this ResearchProjectDef def) where T : ResearchMod => (T)def.GetResearchMod(typeof(T));

        public static bool IsRepeatableResearch(this ResearchProjectDef def) => def.GetResearchMod<ResearchMod_Repeatable>() != null;

        public static void LoadAndEditField(this FieldInfo fieldInfo, string value, object instance)
        {
            object val = Parse(fieldInfo, value);
            fieldInfo.SetValue(instance, val);
        }

        public static object Parse(this FieldInfo fieldInfo, string value)
        {
            object val;
            try
            {
                if (fieldInfo.FieldType.IsSubclassOf(typeof(Def)))
                    val = GenDefDatabase.GetDef(fieldInfo.FieldType, value);
                else
                    val = ParseHelper.FromString(value, fieldInfo.FieldType);
                return val;
            }
            catch (Exception e)
            {
                throw new Exception("Researchable Stat Upgrades :: Exception parsing string: " + e);
            }
        }
    }
}
