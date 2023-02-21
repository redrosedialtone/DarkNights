using System.Collections;
using System.Collections.Generic;
using System;
using Nebula;

namespace Dark.Systems.Tasks
{
    [Flags]
    public enum ITaskStatus
    {
        Nil = 0,
        Cancelled = 1,
        Pending = 2,
        Running = 4,
        Interrupted = 8,
        Success = 16,
        Failure = 32,

        FINISHED = Success|Failure|Cancelled,
        PAUSED = Pending|Interrupted
    }

    public enum ITaskTransition
    {
        Nil = 0,
        Stopping = 1,
        Interrupting = 2,
        Finishing = 3
    }

    public enum TaskAssignmentMethod
    {
        DEFAULT = 0,
        ENQUEUE = 1,
        INTERRUPT = 2,
        CLEAR = 3,
    }

    public interface IWorker
    {
        IWorkOrder CurrentOrder { get; }
        ICreatureNavigation Navigation { get; }
        ICreatureInventory Inventory { get; }
        LinkedList<IWorkOrder> TaskQueue { get; }
        bool Available { get; }

        void AssignTask(IWorkOrder Task, TaskAssignmentMethod AssignmentMode = TaskAssignmentMethod.DEFAULT);
    }

    public interface ITask
    {
        ITaskStatus Status { get; }
        ITaskTransition TransitionState { get; }

        void Tick();

        void SetStatus(ITaskStatus taskState); 
        void Transition(ITaskTransition transitionState);
        void Stop();
        void Execute();
        void Complete();
        void Interrupt();
    }

    public interface IWorkOrder : ITask
    {
        IWorker Worker { get; }
        
        ITaskState CurrentTask { get; }
        ITaskState[] TaskList { get; }

        void Assign(IWorker Worker);       
    }

    public abstract class WorkOrder : IWorkOrder
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[WORKORDER]");

        public ITaskStatus Status { get; protected set; }
        public ITaskTransition TransitionState { get; protected set; }
        public IWorker Worker { get; protected set; }
        public ITaskState CurrentTask { get; protected set; }
        public ITaskState[] TaskList { get; protected set; }

        protected int TaskIndex = 0;
        protected bool Running => (Status & ITaskStatus.Running) != 0;

        public void Assign(IWorker Worker)
        {
            this.Worker = Worker;
        }

        public virtual void SetStatus(ITaskStatus taskState)
        {
            this.Status = taskState;
            if (CurrentTask != null)
            {
                CurrentTask.SetStatus(taskState);
                log.Trace($"Task State::{CurrentTask.Status}");
            }
        }

        public virtual void Transition(ITaskTransition transitionState)
        {
            this.TransitionState = transitionState;
            if (CurrentTask != null)
            {
                CurrentTask.Transition(transitionState);
            }
        }

        public virtual void Stop() { Transition(ITaskTransition.Stopping); }
        public virtual void Execute() { SetStatus(ITaskStatus.Running); }
        public virtual void Complete() { Transition(ITaskTransition.Finishing); }
        public virtual void Interrupt()
        {
            TransitionState = ITaskTransition.Interrupting;
        }

        public void Tick()
        {
            UpdateState();
            if (Running)
            {
                bool Success = false;
                if (CurrentTask != null)
                {
                    CurrentTask.Tick();
                    Success = (CurrentTask.Status & ITaskStatus.FINISHED) != 0;
                }
                if (CurrentTask == null || Success)
                {
                    log.Trace("Checking Next Task..");
                    // Is there another task?
                    if (Next())
                    {
                        // If so, run.
                        CurrentTask.Execute();
                    }
                    else
                    {
                        // Otherwise we're done.
                        Complete();
                    }
                }
            }
        }

        protected void UpdateState()
        {
            if (TransitionState != ITaskTransition.Nil)
            {
                switch (TransitionState)
                {
                    case ITaskTransition.Stopping:
                        if ((CurrentTask.Status & ITaskStatus.FINISHED) != 0)
                        {
                            SetStatus(ITaskStatus.Cancelled);
                        }
                        break;
                    case ITaskTransition.Interrupting:
                        if ((CurrentTask.Status & ITaskStatus.PAUSED) != 0)
                        {
                            SetStatus(ITaskStatus.Interrupted);
                        }
                        break;
                    case ITaskTransition.Finishing:
                        if ((CurrentTask.Status&ITaskStatus.FINISHED)!=0)
                        {
                            SetStatus(CurrentTask.Status);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected bool Next()
        {
            log.Trace("Finding Next Task..");
            if (TaskList != null && TaskIndex < TaskList.Length)
            {
                if (CurrentTask != null)
                {
                    CurrentTask.Complete();
                }
                CurrentTask = TaskList[TaskIndex];
                CurrentTask.Assign(this);
                TaskIndex++;
                return true;
            }
            return false;
        }
    }

    public class MoveToWaypointTask : WorkOrder
    {
        public WorldPoint Destination { get; private set; }

        public MoveToWaypointTask(WorldPoint Destination)
        {
            this.Destination = Destination;
            TaskList = new ITaskState[]
            {
                new MoveTo(Destination)
            };
        }
    }
}
