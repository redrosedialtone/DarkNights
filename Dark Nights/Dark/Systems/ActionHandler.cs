using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark.Systems
{
    public class ActionHandler : IManager
    {
        #region Static
        private static ActionHandler instance;
        public static ActionHandler Get => instance;

        public bool Initialized => throw new System.NotImplementedException();

        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[ACTIONSYS]");
        #endregion

        private void Awake()
        {
            instance = this;
        }

        private readonly Stack<IReversibleAction> _UndoActions = new Stack<IReversibleAction>();
        private readonly Stack<IReversibleAction> _RedoActions = new Stack<IReversibleAction>();

        public static void Redo(int levels) { instance.Instance_Redo(levels); }

        private void Instance_Redo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (_RedoActions.Count != 0)
                {
                    IReversibleAction cmd = _RedoActions.Pop();
                    if (!cmd.Execute())
                    {
                        log.Error("Failed Redo Action!");
                    }
                    _UndoActions.Push(cmd);
                }
            }
        }

        public static void Undo(int levels) { instance.Instance_Undo(levels); }

        private void Instance_Undo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (_UndoActions.Count != 0)
                {
                    IReversibleAction cmd = _UndoActions.Pop();
                    if (!cmd.UnExecute())
                    {
                        log.Error("Failed Undo Action!");
                    }
                    _RedoActions.Push(cmd);
                }
            }
        }

        public static void InsertReversibleAction(IReversibleAction cmd)
        {
            instance.Instance_InsertReversibleAction(cmd);
        }

        private void Instance_InsertReversibleAction(IReversibleAction cmd)
        {
            _UndoActions.Push(cmd);
            _RedoActions.Clear();
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void OnInitialized()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
}
