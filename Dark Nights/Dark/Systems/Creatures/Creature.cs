using Dark.Systems.Tasks;
using Microsoft.Xna.Framework;
using Nebula;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public interface ICreature : IWorldTick, ICreatureRenderData
    {
        ICreatureNavigation Navigation { get; }
        ICreatureAI AI { get; }
        ICreatureInventory Inventory { get; }
    }
}

namespace Dark.Creatures
{
    public abstract class CreatureBase : ICreature
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[CREATURE]");

        public abstract ICreatureNavigation Navigation {get;}
        public ICreatureAI AI { get; protected set; }
        public virtual ICreatureInventory Inventory { get; protected set; }

        public WorldPoint Coordinates => Navigation.Coordinates;
        public Vector2 RenderPoint => Navigation.Position;
        public Vector2 RenderFacing => Navigation.Facing;
        public abstract string RenderTexture { get; }

        public virtual void WorldTick(float delta)
        {

        }

        public virtual void SetTilePosition(WorldPoint Point)
        {
            Navigation.SetTilePosition(Point);
        }
    }

    public class TestCreature : CreatureBase
    {
        public IWorkOrder CurrentOrder { get; private set; }
        public LinkedList<IWorkOrder> TaskQueue { get; } = new LinkedList<IWorkOrder>();
        public bool Available { get; } = true;

        public IVision Vision => _sight;
        private readonly BaseCreatureSight _sight;
        public override ICreatureNavigation Navigation => _navigation;
        private readonly CreatureBaseNavigation _navigation;
        public override string RenderTexture => "character";

        public TestCreature()
        {
            _navigation = new CreatureBaseNavigation(this);
            AI = new TestAI(this);
            Inventory = new TestInventory();
            //_sight = new BaseCreatureSight(this, 16);
            //VisionSystem.Get.AddPlayerSight(this);
        }

        public override void WorldTick(float delta)
        {
            base.WorldTick(delta);
            //if (CurrentOrder != null)
            //{
            //    CurrentOrder.Tick();
            //    if ((CurrentOrder.Status & ITaskStatus.FINISHED) != 0)
            //    {
            //        TaskQueue.RemoveFirst();
            //        CurrentOrder = null;
            //        LookForWork();
            //    }
            //    else if ((CurrentOrder.Status & ITaskStatus.PAUSED) != 0)
            //    {
            //        CurrentOrder = null;
            //        LookForWork();
            //    }
            //}
            AI.Tick();
            _navigation.WorldTick(delta);

        }

        //public void AssignTask(IWorkOrder Task, TaskAssignmentMethod Assignment = TaskAssignmentMethod.DEFAULT)
        //{
        //    this.Verbose("Assigning Task..");
        //    if (Assignment == TaskAssignmentMethod.DEFAULT || Assignment == TaskAssignmentMethod.ENQUEUE)
        //    {
        //        TaskQueue.AddLast(Task);
        //        LookForWork();
        //    }
        //    else if (Assignment == TaskAssignmentMethod.INTERRUPT)
        //    {
        //        if (CurrentOrder != null)
        //        {
        //            CurrentOrder.Interrupt();                  
        //        }
        //        TaskQueue.AddFirst(Task);
        //    }
        //    else if (Assignment == TaskAssignmentMethod.CLEAR)
        //    {
        //        if (CurrentOrder != null)
        //        {
        //            CurrentOrder.Stop();
        //        }
        //        TaskQueue.Clear();
        //        TaskQueue.AddFirst(Task);
        //    }
            
        //}

        //private void LookForWork()
        //{
        //    if (CurrentOrder == null)
        //    {
        //        if (TaskQueue.Count > 0)
        //        {
        //            BeginTask(TaskQueue.First.Value);
        //        }
        //    }
        //}

        //private void BeginTask(IWorkOrder Task)
        //{
        //    CurrentOrder = Task;
        //    Task.Assign(this);
        //    Task.Execute();
        //}
    }
}
