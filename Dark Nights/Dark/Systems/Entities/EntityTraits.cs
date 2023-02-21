using Dark.Entites;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public interface IEntityTrait : IEntityTriggerSystem, IEntityModuleSearch, IEntityTaskSystem
    {
        string TRAIT_DEF_NAME { get; }
        IEntityModule[] TraitModules { get; }
    }

    public interface ITraitContainable
    {
        ICreatureInventory Inventory { get; }
        int Encumberance { get; }

        void Contained(ICreatureInventory Inventory);
    }

    public interface IEntityNavData
    {
        int Cost { get; }
    }

    public interface IWalkNavData : IEntityNavData
    {
        bool Walkable { get; }
    }

    public interface IVisibility
    {
        float Opacity { get; set; }
    }

}

namespace Dark.Entities
{
    public abstract class EntityBaseTrait : IEntityTrait
    {
        public abstract string TRAIT_DEF_NAME { get; }
        public abstract IEntityModule[] TraitModules { get; }

        public virtual void Trigger(IEntityTrigger Event)
        {
            if (TraitModules != null)
            {
                foreach (var module in TraitModules)
                {
                    if (module is IEntityTriggerSystem _trigger)
                    {
                        _trigger.Trigger(Event);
                    }
                }
            }
        }

        public virtual ModuleType FindModule<ModuleType>() where ModuleType : IEntityModule, new()
        {
            if (TraitModules != null)
            {
                foreach (var module in TraitModules)
                {
                    if (module is ModuleType _match)
                    {
                        return _match;
                    }
                }
            }
            return default;
        }

        public virtual void AssignTask(EntityTask Task)
        {
            if (this is IEntityTaskWorker Worker)
            {
                Task.Execute(Worker);
            }
            if (TraitModules != null)
            {
                foreach (var module in TraitModules)
                {
                    if (module is IEntityTaskWorker ModuleWorker)
                    {
                        Task.Execute(ModuleWorker);
                    }
                }
            }
        }
    }

    public class Trait_HasMaterial : EntityBaseTrait
    {
        public override string TRAIT_DEF_NAME => "HAS_MATERIAL";
        public override IEntityModule[] TraitModules { get; }

        public IEntityMaterialDef MaterialDef;

        public Trait_HasMaterial(IEntityMaterialDef MaterialDef)
        {
            this.MaterialDef = MaterialDef;
        }
    }

    public class Trait_Ground : EntityBaseTrait, ITaskWorker_GetTileName, ITaskWorker_GetEntityPlacementFlags, IWalkNavData
    {
        public override string TRAIT_DEF_NAME => "TILE";
        public override IEntityModule[] TraitModules { get; }
        public int Cost { get; private set; }
        public bool Walkable { get; private set; }

        public EntityPlacementFlags PlacementFlags { get; private set; }
        protected string Name;

        public Trait_Ground(string Name, bool Navigable = true, int cost = 1)
        {
            this.Name = Name;
            Walkable = Navigable;
            Cost = cost;

            PlacementFlags = new EntityPlacementFlags((int)EntityLayer.TILE,
                EntityHeightRule.ReplaceIf | EntityHeightRule.LesserOrEqual,
                EntityHeightRule.Forbid | EntityHeightRule.LesserOrEqual);
        }

        public string GetTileName()
        {
            return Name;
        }
    }

    public class Trait_Impassable : EntityBaseTrait, IWalkNavData, ITaskWorker_GetEntityPlacementFlags
    {
        public override string TRAIT_DEF_NAME => "IMPASSABLE";
        public override IEntityModule[] TraitModules { get; }
        public int Cost { get; private set; } = 0;
        public bool Walkable { get; private set; } = false;

        public EntityPlacementFlags PlacementFlags { get; } = new EntityPlacementFlags((int)EntityLayer.CONSTRUCTS,
            EntityHeightRule.Forbid | EntityHeightRule.GreaterOrEqual,
            EntityHeightRule.Forbid | EntityHeightRule.GreaterOrEqual);

        public override void Trigger(IEntityTrigger Event)
        {
            if (Event is EntityTrigger_OnTileEnter EntryEvent)
            {
                ITileNavData navData = EntryEvent.Data.NavData[NavigationMode.Walking];
                navData.NavEntityAdded(this);


            }
            else if (Event is EntityTrigger_OnTileExit ExitEvent)
            {
                ITileNavData navData = ExitEvent.Data.NavData[NavigationMode.Walking];
                navData.NavEntityRemoved(this);
            }
        }
    }

    public class Trait_Visibility : EntityBaseTrait, IVisibility
    {
        public override string TRAIT_DEF_NAME => "OPAQUE";
        public override IEntityModule[] TraitModules { get; }
        public float Opacity { get; set; } = 0;

        public Trait_Visibility(float Opacity)
        {
            this.Opacity = Opacity;
        }

        public override void Trigger(IEntityTrigger Event)
        {
            if (Event is EntityTrigger_OnTileEnter EntryEvent)
            {
                //Debug.Log($"EntityTrigger::{EntryEvent == null}");
                //Debug.Log($"EntityTrigger::{EntryEvent.Data == null}");
                //Debug.Log($"EntityTrigger::{EntryEvent.Data.VisibilityData == null}");
                //Debug.Log($"EntityTrigger::{EntryEvent.Data.VisibilityData.Opacity}::{this.Opacity}");
                EntryEvent.Data.VisibilityData.Opacity = this.Opacity;           
            }
            //TODO: On removed
        }
    }



    public class Trait_Item : EntityBaseTrait, ITraitContainable
    {
        public override string TRAIT_DEF_NAME => "ITEM";
        public override IEntityModule[] TraitModules { get; }

        public ICreatureInventory Inventory { get; private set; }
        public int Encumberance { get; }

        public Trait_Item(int Encumberance)
        {
            this.Encumberance = Encumberance;
        }

        public void Contained(ICreatureInventory Inventory)
        {
            this.Inventory = Inventory;
        }
    }
}
    