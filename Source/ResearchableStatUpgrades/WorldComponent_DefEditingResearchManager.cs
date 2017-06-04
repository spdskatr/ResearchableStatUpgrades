using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ResearchableStatUpgrades
{
    public interface IUndoable
    {
        void Apply();
        void Undo(object val);
    }
    public class GameComponent_DefEditingResearchManager : GameComponent
    {
        public GameComponent_DefEditingResearchManager() { }
        public GameComponent_DefEditingResearchManager(Game game) { }
        public Dictionary<IUndoable, object> undoables;
        public void ApplyAndRecord(Action a, IUndoable instance, object oldVal)
        {
            //Cache old value
            undoables.Add(instance, oldVal);
            //Execute
            a();
        }
    }
}
