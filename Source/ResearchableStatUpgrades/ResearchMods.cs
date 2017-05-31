using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ResearchableStatUpgrades
{
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

    public class ResearchMod_EditVerbProperties : ResearchMod
    {
        public const BindingFlags universal = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public ThingDef def;
        public int index;
        public string fieldName;
        public string value;
        public override void Apply()
        {
            var localVerbs = def.Verbs;
            FieldInfo fieldInfo = typeof(VerbProperties).GetField(fieldName, universal);
            fieldInfo.SetValue(def.Verbs[index], ParseHelper.FromString(value, fieldInfo.FieldType));
        }
    }
    public class ResearchMod_EditBuildingProperties : ResearchMod
    {
        public const BindingFlags universal = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public ThingDef def;
        public string fieldName;
        public string value;
        public override void Apply()
        {
            FieldInfo fieldInfo = typeof(BuildingProperties).GetField(fieldName, universal);
            fieldInfo.SetValue(def.building, ParseHelper.FromString(value, fieldInfo.FieldType));
        }
    }
    public class ResearchMod_EditDef : ResearchMod
    {
        public Def def;
        public string fieldName;
        public string value;
        public override void Apply()
        {
            FieldInfo fieldInfo = def.GetType().GetFields(ResearchMod_EditBuildingProperties.universal).ToList().Find(field => field.Name == fieldName);
            if (fieldInfo == null)
                Log.Error(string.Format("Cannot find field {0} in Def {1}", fieldName, def.defName));
            fieldInfo.SetValue(def, ParseHelper.FromString(value, fieldInfo.FieldType));
        }
    }
}
