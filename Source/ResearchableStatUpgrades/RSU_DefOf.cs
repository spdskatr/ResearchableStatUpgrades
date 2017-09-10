using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Harmony;//Only for research temp fix
using System.Reflection.Emit;

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
    //SECTION BELOW IS PATCH FOR STACKING RESEARCH
    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup)), StaticConstructorOnStartup]
    static class Patch
    {
        public static readonly HarmonyInstance inst;
        static Patch()
        {
            inst = HarmonyInstance.Create("com.spdskatr.rsu.fixes");
            inst.Patch(typeof(Thing).GetMethod("SpawnSetup"), null, null, HarMetCtor("Transpiler1"));
            inst.Patch(typeof(CompUseEffect_Artifact).GetMethod("DoEffect"), null, null, HarMetCtor("Transpiler2"));
            inst.Patch(typeof(CompUseEffect_LearnSkill).GetMethod("DoEffect"), null, null, HarMetCtor("Transpiler2"));
            Log.Message("Researchable Stat Upgrades :: Fix(es) initialized (abolished spawn stack count truncation entirely, fixed bug where using neurotrainers/artifacts more than once caused whole stack to deplete)");
        }
        static HarmonyMethod HarMetCtor(MethodInfo method)
        {
            return new HarmonyMethod(method);
        }
        static HarmonyMethod HarMetCtor(string method, Type type = null, Type[] args = null)
        {
            return new HarmonyMethod(type ?? typeof(Patch), method, args);
        }
        static MethodInfo GetLocalMethod(string method, Type[] parameters = null)
        {
            return AccessTools.Method(typeof(Patch), method, parameters);
        }
        static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            List<CodeInstruction> instr = new List<CodeInstruction>(instructions);

            // find 1st use of 'ThingDef::stackLimit'. It's the 'if (this.stackCount > this.def.stackLimit)' comparison. Modify following jump to always skip the correcting code block inside the 'if'
            // real clean version would then do a 2nd pass over all skipped things once the resaerch modifiers are correctly loaded... but.... ah well... it'll work out... hopefully...
            var idxFirstLimitReference = instr.FirstIndexOf(ci => ci.opcode == OpCodes.Ldfld && ci.operand == typeof(ThingDef).GetField(nameof(ThingDef.stackLimit)));
            if (idxFirstLimitReference == -1 || instr[idxFirstLimitReference +1].opcode != OpCodes.Ble) {
                Log.Warning("Could not find expected 'stackLimit' reference - not patching SpawnSetup.");
                return instr;
            }
            instr[idxFirstLimitReference + 1].opcode = OpCodes.Br;

            return instr;
        }
        //2nd patch
        static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> original, ILGenerator gen)
        {
            var label = gen.DefineLabel();
            var label2 = gen.DefineLabel();
            bool addlabel = false;
            foreach (var cur in original)
            {
                if (cur.operand == typeof(ThingWithComps).GetMethod("Destroy"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, GetLocalMethod("TranspilerUtility"));
                    yield return new CodeInstruction(OpCodes.Brfalse, label);
                    yield return cur;
                    yield return new CodeInstruction(OpCodes.Br, label2);
                    yield return new CodeInstruction(OpCodes.Call, GetLocalMethod("Absorb")).AddLabel(label);
                    addlabel = true;
                }
                else if (addlabel)
                {
                    yield return cur.AddLabel(label2);
                    addlabel = false;
                }
                else
                {
                    yield return cur;
                }
            }
        }
        public static CodeInstruction AddLabel(this CodeInstruction instr, params Label[] ls)
        {
            instr.labels.AddRange(ls);
            return instr;
        }
        public static void Absorb(ThingComp comp, DestroyMode mode) { }
        public static bool TranspilerUtility(ThingComp tc)
        {
            var t = tc.parent;
            if (t.stackCount > 1)
            {
                t.SplitOff(1);
                return false;
            }
            return true;
        }
    }
}
