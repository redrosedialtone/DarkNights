using Dark.Systems.Tasks;
using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class TaskSystem : IManager
    {
        #region Static
        private static TaskSystem instance;
        public static TaskSystem Get => instance;

        public bool Initialized => throw new System.NotImplementedException();

        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[TASKSYS]");

        #endregion

        public Queue<IWorkOrder> TaskQueue = new Queue<IWorkOrder>();
        private List<IWorker> workers = new List<IWorker>();

        public void Init()
        {
            log.Info("> Task System Init.. <");
            instance = this;
        }

        public static void Delegate(IWorkOrder Order)
        {
            instance.TaskDelegated(Order);
        }

        private void TaskDelegated(IWorkOrder Order)
        {
            log.Trace("Delegating Work..");
            foreach (var worker in workers)
            {
                if (worker.Available)
                {
                    AssignTaskTo(Order, worker, TaskAssignmentMethod.DEFAULT);
                    return;
                }
            }
            TaskQueue.Enqueue(Order);
        }

        public static void Assign(IWorkOrder Task, IWorker Worker, TaskAssignmentMethod AssignmentMode = TaskAssignmentMethod.DEFAULT)
        {
            instance.AssignTaskTo(Task, Worker, AssignmentMode);
        }

        private void AssignTaskTo(IWorkOrder Task, IWorker Worker, TaskAssignmentMethod AssignmentMode)
        {
            log.Trace("Assigning Task to Worker..");
            Worker.AssignTask(Task, AssignmentMode);
        }

        public static void Worker(IWorker Worker)
        {
            instance.NewWorker(Worker);
        }

        private void NewWorker(IWorker Worker)
        {
            log.Trace("Available Worker Added!");
            workers.Add(Worker);
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
