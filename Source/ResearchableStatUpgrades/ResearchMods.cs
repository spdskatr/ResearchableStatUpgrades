using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ResearchableStatUpgrades
{
    public interface IRegisterable { void Register(WorldComponent_DefEditingResearchManager comp); }
    
    public abstract class ResearchMod_SingleRegisterable : ResearchMod, IRegisterable
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
            //For testing and spamming purposes - No real effect on standard gameplay - AND CAUSING BUGS!
            //Find.ResearchManager.currentProj = null;
        }
    }
    public class ResearchMod_Repeatable : ResearchMod_SetResearchToZero
    {
        public override void Apply()
        {
            base.Apply();
            RSUUtil.RepeatableResearchManager.AddToDictionary(def);
        }
    }

    public class ResearchMod_EditVerbProperties : ResearchMod_SingleRegisterable
    {
        public ThingDef def;
        public int index;
        public string fieldName;
        public string value;
        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = typeof(VerbProperties).GetField(fieldName, RSUUtil.universal);
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def.Verbs[index]), RSUUtil.Parse(fieldInfo, value), def.Verbs[index]);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditBuildingProperties : ResearchMod_SingleRegisterable
    {
        public ThingDef def;
        public string fieldName;
        public string value;
        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = typeof(BuildingProperties).GetField(fieldName, RSUUtil.universal);
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def.building), RSUUtil.Parse(fieldInfo, value), def.building);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditDef : ResearchMod_SingleRegisterable
    {
        public Def def;
        public string fieldName;
        public string value;

        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = def.GetType().GetFields(RSUUtil.universal).ToList().Find(field => field.Name == fieldName);
            if (fieldInfo == null)
                Log.Error(string.Format("Cannot find field {0} in Def {1}", fieldName, def.defName));
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def), RSUUtil.Parse(fieldInfo, value), def);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditIngestibleProperties : ResearchMod_SingleRegisterable
    {
        public ThingDef def;
        public string fieldName;
        public string value;

        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = typeof(IngestibleProperties).GetField(fieldName, RSUUtil.universal);
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(def.ingestible), RSUUtil.Parse(fieldInfo, value), def.ingestible);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }
    public class ResearchMod_EditCompProperties : ResearchMod_SingleRegisterable
    {
        public ThingDef def;
        public Type type;
        public string fieldName;
        public string value;

        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            WorldComp = comp;
            FieldInfo fieldInfo = type.GetField(fieldName, RSUUtil.universal);
            
            var compProps = def.comps.Find(c => c.GetType() == type || c.GetType().IsSubclassOf(type));
            if (compProps == null)
            {
                Log.Error("CompProperties type " + type.FullName + " was not found.");
                return;
            }
            LogicFieldEditor logicFieldEditor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(compProps), RSUUtil.Parse(fieldInfo, value), compProps);
            Editor = logicFieldEditor;
            WorldComp.AddEditor(Editor, false);
        }
    }

    public class ResearchMod_EditStackCounts : ResearchMod
    {
        public override void Apply()
        {
            RSUUtil.StackCountEditManager.RefreshResearches();
        }
    }
}
