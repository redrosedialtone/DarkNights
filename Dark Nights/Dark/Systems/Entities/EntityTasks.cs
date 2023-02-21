using System;
using System.Collections;
using System.Collections.Generic;

namespace Dark.Entities
{
    public interface IEntityTask
    {
        public bool Completed { get; }
        public void Execute(IEntityTaskWorker Worker);
    }

    public interface IEntityTaskManager
    {
        bool TaskResponse(IEntityTaskWorker Worker, EntityTask Task);
    }

    public interface IEntityTaskWorker
    {
        
    }

    public abstract class EntityTask : IEntityTask
    {
        protected IEntityTaskManager Manager;

        public abstract void Execute(IEntityTaskWorker Worker);
        public bool Completed { get; protected set; }

        public EntityTask(IEntityTaskManager Manager)
        {
            this.Manager = Manager;
        }

        protected virtual bool Running { get; set; } = true;

        protected virtual bool CompleteTask(IEntityTaskWorker Worker)
        {
            Completed = Running = Manager.TaskResponse(Worker, this);
            return Completed;
        }
        
    }

    public interface ITaskWorker_GetTileName : IEntityTaskWorker
    {
        string GetTileName();
    }

    public class Task_GetTileName : EntityTask
    {
        public string Name;

        public Task_GetTileName(IEntityTaskManager Manager) : base(Manager) { }

        public override void Execute(IEntityTaskWorker Worker)
        {
            if (Running && Worker is ITaskWorker_GetTileName TileNameWorker)
            {
                Name = TileNameWorker.GetTileName();
                CompleteTask(TileNameWorker);
            }
        }
    }

    public interface ITaskWorker_GetEntityMaterial : IEntityTaskWorker
    {
        string GetMaterialName { get; }
    }

    public class Task_GetEntityMaterial : EntityTask
    {
        public string TextureName;

        public Task_GetEntityMaterial(IEntityTaskManager Manager) : base(Manager) { }

        public override void Execute(IEntityTaskWorker Worker)
        {
            if (Worker is ITaskWorker_GetEntityMaterial EntitySpriteWorker)
            {
                TextureName = EntitySpriteWorker.GetMaterialName;
                CompleteTask(EntitySpriteWorker);
            }
        }
    }

    public interface ITaskWorker_GetEntityPlacementFlags : IEntityTaskWorker
    {
        EntityPlacementFlags PlacementFlags { get; }
    }
    
    public class Task_GetEntityPlacementFlags : EntityTask
    {
        public EntityPlacementFlags[] PlacementFlags => _placementFlags.ToArray();
        private List<EntityPlacementFlags> _placementFlags = new List<EntityPlacementFlags>();

        public Task_GetEntityPlacementFlags(IEntityTaskManager Manager) : base(Manager) { }

        public override void Execute(IEntityTaskWorker Worker)
        {
            if (Worker is ITaskWorker_GetEntityPlacementFlags EntityPlacementWorker)
            {
                if (Running)
                {
                    _placementFlags.Add(EntityPlacementWorker.PlacementFlags);
                    CompleteTask(EntityPlacementWorker);
                }                
            }
        }

        protected override bool CompleteTask(IEntityTaskWorker Worker)
        {
            Completed = true;
            Running = Manager.TaskResponse(Worker, this);
            return Completed;
        }
    }
}
