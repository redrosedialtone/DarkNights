using Nebula;
using System.Collections;
using System.Collections.Generic;

namespace Dark.Systems.Tasks
{

    public interface ITaskState : ITask
    {
        IWorkOrder WorkOrder { get; }

        void Assign(IWorkOrder Order);
    }

    public abstract class TaskState : ITaskState
    {
        public IWorkOrder WorkOrder { get; private set; }
        public ITaskStatus Status { get; private set; }
        public ITaskTransition TransitionState { get; protected set; }

        protected bool Running => (Status&ITaskStatus.Running) != 0;
        protected IWorker Worker => WorkOrder.Worker;

        protected ITaskStatus FinishState;

        public abstract void Execute();


        public virtual void Assign(IWorkOrder Order)
        {
            this.WorkOrder = Order;
        }

        public virtual void SetStatus(ITaskStatus State)
        {
            this.Status = State;
        }

        public virtual void Tick()
        {
            UpdateState(); 
        }

        protected virtual void UpdateState()
        {
            if (TransitionState != ITaskTransition.Nil)
            {
                switch (TransitionState)
                {
                    case ITaskTransition.Stopping:
                        SetStatus(ITaskStatus.Cancelled);
                        break;
                    case ITaskTransition.Interrupting:
                        SetStatus(ITaskStatus.Interrupted);
                        break;
                    case ITaskTransition.Finishing:
                        SetStatus(FinishState);
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void Stop()
        {
            Transition(ITaskTransition.Stopping);
        }

        public virtual void Complete()
        {
            FinishState = ITaskStatus.Success;
            Transition(ITaskTransition.Finishing);
        }

        public virtual void Interrupt()
        {
            Transition(ITaskTransition.Interrupting);
        }

        public virtual void Transition(ITaskTransition transitionState)
        {
            this.TransitionState = transitionState;
        }
    }

    public class MoveTo : TaskState
    {
        private readonly WorldPoint Destination;

        public MoveTo(WorldPoint Destination)
        {
            this.Destination = Destination;
            SetStatus(ITaskStatus.Pending);
        }

        public override void Execute()
        {
            Worker.Navigation.Destination(Destination);
            SetStatus(ITaskStatus.Running);
        }

        public override void Tick()
        {
            UpdateState();
            if (Running)
            {
                if (WorkOrder.Worker.Navigation.Coordinates == Destination)
                {
                    Transition(ITaskTransition.Finishing);
                }
            }
        }

        protected override void UpdateState()
        {
            if (TransitionState != ITaskTransition.Nil)
            {
                switch (TransitionState)
                {
                    case ITaskTransition.Stopping:
                        FinishState = ITaskStatus.Cancelled;
                        Cancel();
                        break;
                    case ITaskTransition.Interrupting:
                        FinishState = ITaskStatus.Interrupted;
                        Cancel();
                        break;
                    case ITaskTransition.Finishing:
                        SetStatus(ITaskStatus.Success);
                        break;
                    default:
                        break;
                }
            }
        }

        private void Cancel()
        {
            Worker.Navigation.Stop(FinaliseMovement);
        }

        private void FinaliseMovement()
        {
            SetStatus(FinishState);
        }
    }
}
