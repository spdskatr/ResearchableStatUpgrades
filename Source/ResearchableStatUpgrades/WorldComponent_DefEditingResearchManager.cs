using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace ResearchableStatUpgrades
{
    public static class RSUCache
    {
        public const string DefEditingResearchManagerSaveString = "WC_DERM_Dictionary";
        public static Dictionary<string, object> cache = new Dictionary<string, object>();
        public static void TryChangeOrAddValue(string str, object val)
        {
            if (cache.ContainsKey(str))
                cache[str] = val;
            else
                cache.Add(str, val);
        }
    }
    public class WorldComponent_DefEditingResearchManager : WorldComponent
    {
        public bool initialized;
        Dictionary<LogicFieldEditor, bool> editors = new Dictionary<LogicFieldEditor, bool>();
        public FieldInfo ResearchModField { get; }
        public World World { get; }
        public WorldComponent_DefEditingResearchManager(World world) : base(world)
        {
            World = world;
            FieldInfo fieldInfo = typeof(ResearchProjectDef).GetField("researchMods", DefEditing.universal);
            ResearchModField = fieldInfo;

            //Tries to find cached dictionary from previous save. The dictionaries aren't save-specific, but still do contain crucial information about the initial value of edited fields.
            if (RSUCache.cache?.ContainsKey(RSUCache.DefEditingResearchManagerSaveString) ?? false)
            {
                editors = (Dictionary<LogicFieldEditor, bool>)RSUCache.cache[RSUCache.DefEditingResearchManagerSaveString];
            }
            //If no dictionary found, create one
            else
            {
                IEnumerable<ResearchMod> enumerable = DefDatabase<ResearchProjectDef>.AllDefs.SelectMany(d => (List<ResearchMod>)fieldInfo.GetValue(d) ?? new List<ResearchMod>());
                foreach (var m in enumerable)
                {
                    if (m is ResearchMod_Registerable r)
                    {
                        r.Register(this);
                    }
                }
            }
        }

        /// <summary>
        /// Destructor caches the editor dictionary just in case. The dictionaries aren't save-specific, but still do contain crucial information about the initial value of edited fields.
        /// </summary>
        ~WorldComponent_DefEditingResearchManager()
        {
            RSUCache.TryChangeOrAddValue(RSUCache.DefEditingResearchManagerSaveString, editors);
        }
        public void AddEditor(LogicFieldEditor a, bool b)
        {
            editors.Add(a, b);
        }
        public void SetEditorValue(LogicFieldEditor a, bool b)
        {
            if (!editors.ContainsKey(a))
            {
                throw new ArgumentException("LogicFieldEditor was not found in the dictionary.");
            }
            editors[a] = b;
            if (initialized)
            {
                a.Resolve(b);
            }
        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            ResearchFinalizeInit();
        }
        private void ResearchFinalizeInit()
        {
            foreach (var editor in editors)
            {
                editor.Key.Resolve(editor.Value);
            }
            initialized = true;
        }
    }
    public class LogicFieldEditor
    {
        public readonly FieldInfo address;
        public readonly object valueFalse;
        public readonly object valueTrue;
        public readonly object instance;
        public LogicFieldEditor(FieldInfo address, object valueFalse, object valueTrue, object instance)
        {
            this.instance = instance;
            this.address = address;
            this.valueFalse = valueFalse;
            this.valueTrue = valueTrue;
        }
        public void Resolve(bool b)
        {
            address.SetValue(instance, b ? valueTrue : valueFalse);
        }
    }
}
