using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ResearchableStatUpgrades
{
    public static class DefEditing
    {
        public const BindingFlags universal = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
        public static readonly MethodBase memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", universal);
        public static void LoadAndEditField(FieldInfo fieldInfo, string value, object instance)
        {
            object val = Parse(fieldInfo, value);
            fieldInfo.SetValue(instance, val);
        }

        public static object MemberwiseClonePublic(this object obj)
        {
            return memberwiseCloneMethod.Invoke(obj, null);
        }

        public static object Parse(FieldInfo fieldInfo, string value)
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
    public abstract class ResearchMod_Registerable : ResearchMod
    {
        public abstract void Register(WorldComponent_DefEditingResearchManager comp);
        public WorldComponent_DefEditingResearchManager WorldComp { get; protected set; }
        public LogicFieldEditor Editor { get; protected set; }
        public override void Apply()
        {
            if (WorldComp != null && Editor != null)
                WorldComp.SetEditorValue(Editor, true);
        }
    }

    public class ResearchMod_SetResearchToZero : ResearchMod
    {
        /// <summary>
        /// Defines a specific def. Leave this null for current def.
        /// </summary>
        public ResearchProjectDef def;
        public override void Apply()
        {
            var progress = (Dictionary<ResearchProjectDef, float>)typeof(ResearchManager).GetField("progress", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Find.ResearchManager);
            if (def == null)
                def = Find.ResearchManager.currentProj;

            progress[def] = 0f;
            //For testing and spamming purposes - No real effect on standard gameplay
            Find.ResearchManager.currentProj = null;
        }
    }
    public class ResearchMod_Repeatable : ResearchMod_SetResearchToZero
    {
        public override void Apply()
        {
            base.Apply();
            var gamecomp = Find.World.GetComponent<WorldComponent_RepeatableResearchManager>();
            gamecomp.AddToDictionary(def);
        }
    }

    public class ResearchMod_EditVerbProperties : ResearchMod_Registerable
    {
        public ThingDef def;
        public int index;
        public string fieldName;
        public string value;
        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = typeof(VerbProperties).GetField(fieldName, DefEditing.universal);
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def.Verbs[index]), DefEditing.Parse(fieldInfo, value), def.Verbs[index]);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditBuildingProperties : ResearchMod_Registerable
    {
        public ThingDef def;
        public string fieldName;
        public string value;
        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = typeof(BuildingProperties).GetField(fieldName, DefEditing.universal);
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def.building), DefEditing.Parse(fieldInfo, value), def.building);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditDef : ResearchMod_Registerable
    {
        public Def def;
        public string fieldName;
        public string value;

        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = def.GetType().GetFields(DefEditing.universal).ToList().Find(field => field.Name == fieldName);
            if (fieldInfo == null)
                Log.Error(string.Format("Cannot find field {0} in Def {1}", fieldName, def.defName));
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def), DefEditing.Parse(fieldInfo, value), def);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditIngestibleProperties : ResearchMod_Registerable
    {
        public ThingDef def;
        public string fieldName;
        public string value;

        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = typeof(IngestibleProperties).GetField(fieldName, DefEditing.universal);
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def.ingestible), DefEditing.Parse(fieldInfo, value), def.ingestible);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
}
