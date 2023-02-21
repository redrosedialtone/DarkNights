using System;
using Dark;
using Dark.Entities;

namespace Dark
{
    public interface IEntityTriggerSystem
    {
        void Trigger(IEntityTrigger Event);
    }

    public interface IEntityModuleSearch
    {
        ModuleType FindModule<ModuleType>() where ModuleType : IEntityModule, new();
    }

    public interface IEntityTaskSystem
    {
        void AssignTask(EntityTask Task);
    }

    public interface IEntityHasMaterial
    {
        IEntityMaterialDef Material { get; }
    }

    public struct EntityID
    {
        public int ID;

        public EntityID(int ID)
        {
            this.ID = ID;
        }

        public static implicit operator int(EntityID EntityID) { return EntityID.ID; }
        public static explicit operator EntityID(int ID) { return new EntityID(ID); }

        public override string ToString()
        {
            return ID.ToString();
        }
    }

    public enum EntityType
    {
        Nil = 0,
        FLOOR = 1,
        FURNITURE = 2,
        CONSTRUCT = 3,
        ITEM = 4,
    }

    /// <summary>
    /// Game Object
    /// </summary>
    public interface IEntity : IEntityTriggerSystem, IEntityModuleSearch, IEntityTaskSystem
    {
        string DefName { get; }
        EntityID ID { get; set; }
        IEntityTrait[] Traits { get; }
        IEntityGraphics EntityGraphicsDef { get; }
        EntityType EntityType { get; }

        IEntityTrait FindTrait<TraitType>();
    }
}

namespace Dark.Entities { 

    public abstract class EntityBase : IEntity
    {
        public abstract string DefName { get; }
        public EntityID ID { get; set; }
        public abstract IEntityTrait[] Traits { get; }
        public IEntityGraphics EntityGraphicsDef { get; protected set; }
        public abstract EntityType EntityType { get; }  

        protected void EntityNavData(NavigationMode Mode)
        {

        }

        public void Trigger(IEntityTrigger Event)
        {
            if (Traits != null)
            {
                foreach (var trait in Traits)
                {
                    trait.Trigger(Event);
                }
            }          
        }

        public IEntityTrait FindTrait<TraitType>()
        {
            if (Traits != null)
            {
                foreach (var trait in Traits)
                {
                    if (trait is TraitType)
                    {
                        return trait;
                    }
                }
            }
            return default;
        }

        public ModuleType FindModule<ModuleType>() where ModuleType : IEntityModule, new()
        {
            if (Traits != null)
            {
                foreach (var trait in Traits)
                {
                    var _ret = trait.FindModule<ModuleType>();
                    if (_ret != null) return _ret;
                }
            }
            return default;
        }

        public void AssignTask(EntityTask Task)
        {
            if (Traits != null)
            {
                foreach (var trait in Traits)
                {
                    trait.AssignTask(Task);
                }
            }
        }
    }

    [Flags]
    public enum EntityHeightRule
    {
        Nil = 0,
        Equal = 1,
        Lesser = 2,
        Greater = 4,

        Forbid = 8,
        Allow = 16,

        Replace = 32,

        GreaterOrEqual = Greater|Equal,
        LesserOrEqual = Lesser|Equal,

        ReplaceIf = Replace|Allow,

        OPERATORS = Equal|Lesser|Greater,
        QUALIFIERS = Forbid|Allow
    }

    public struct EntityPlacementFlags : IEntityTaskManager
    {
        public int Height;
        // When this entity is placed
        public EntityHeightRule _onPlacement;
        // When another entity is placed
        public EntityHeightRule _allowPlacement;

        public EntityPlacementFlags(int Height, EntityHeightRule OnPlacement, EntityHeightRule AllowPlacement)
        {
            this.Height = Height; 
            this._onPlacement = OnPlacement; this._allowPlacement = AllowPlacement;
            if ((OnPlacement & EntityHeightRule.QUALIFIERS) == 0)
            {
                this._onPlacement |= EntityHeightRule.Allow;
            }
        }

        public bool OnPlacement(IEntity Other)
        {
            Task_GetEntityPlacementFlags entityPlacementFlags = new Task_GetEntityPlacementFlags(this);
            Other.AssignTask(entityPlacementFlags);

            bool outcome = true;
            if (entityPlacementFlags.Completed)
            {
                foreach (var flag in entityPlacementFlags.PlacementFlags)
                {
                    var _ret = OnPlacement(flag);
                    outcome = outcome && _ret;
                }
            }

            return outcome;
        }

        public bool OnPlacement(EntityPlacementFlags Other)
        {
            EntityHeightRule operation = (_onPlacement & EntityHeightRule.OPERATORS);
            EntityHeightRule qualifier = (_onPlacement & EntityHeightRule.QUALIFIERS);

            bool equalityCheck = (operation & EntityHeightRule.Equal) != 0;
            bool result = false;

            //Debug.Log($"CHECK::{this.Height}v{Other.Height}");

            if ((operation & EntityHeightRule.Lesser) != 0)
            {
                result = equalityCheck ? Other.Height <= this.Height : Other.Height < this.Height;
                //Debug.Log($"[L{(equalityCheck ? "E" : "")}]{(result ? "PASS" : "FAIL")}");
            }
            else if ((operation & EntityHeightRule.Greater) != 0)
            {
                result = equalityCheck ? Other.Height >= this.Height : Other.Height > this.Height;
                //Debug.Log($"[G{(equalityCheck ? "E" : "")}]{(result ? "PASS" : "FAIL")}");
            }
            else if (equalityCheck)
            {
                result = Other.Height == this.Height;
                //Debug.Log($"[E]{(result ? "PASS" : "FAIL")}");
            }

            if ((qualifier & EntityHeightRule.Forbid) != 0)
            {
                //Debug.Log($"[F]{(!result ? "PASS" : "FAIL")}");
                return !result;
            }
            //Debug.Log($"[A]{(result ? "PASS" : "FAIL")}");
            return result;
        }

        public bool AllowPlacement(IEntity Other)
        {
            Task_GetEntityPlacementFlags entityPlacementFlags = new Task_GetEntityPlacementFlags(this);
            Other.AssignTask(entityPlacementFlags);

            bool outcome = true;
            if (entityPlacementFlags.Completed)
            {
                foreach (var flag in entityPlacementFlags.PlacementFlags)
                {
                    var _ret = AllowPlacement(flag);
                    outcome = outcome && _ret;
                }
            }

            return outcome;
        }

        public bool AllowPlacement(EntityPlacementFlags Other)
        {
            EntityHeightRule operation = (_allowPlacement & EntityHeightRule.OPERATORS);
            EntityHeightRule qualifier = (_allowPlacement & EntityHeightRule.QUALIFIERS);

            bool equalityCheck = (operation & EntityHeightRule.Equal) != 0;
            bool result = false;

            //Debug.Log($"CHECK::{this.Height}v{Other.Height}");

            if ((operation & EntityHeightRule.Lesser) != 0)
            {
                result = equalityCheck ? Other.Height <= this.Height : Other.Height < this.Height;
                //Debug.Log($"[L{(equalityCheck ? "E" : "")}]{(result ? "PASS" : "FAIL")}");
            }
            else if ((operation & EntityHeightRule.Greater) != 0)
            {
                result = equalityCheck ? Other.Height >= this.Height : Other.Height > this.Height;
                //Debug.Log($"[G{(equalityCheck ? "E" : "")}]{(result ? "PASS" : "FAIL")}");
            }
            else if(equalityCheck)
            {
                result = Other.Height == this.Height;
                //Debug.Log($"[E]{(result ? "PASS" : "FAIL")}");
            }

            if ((qualifier & EntityHeightRule.Forbid) != 0)
            {
                //Debug.Log($"[F]{(!result ? "PASS" : "FAIL")}");
                return !result;
            }
            //Debug.Log($"[A]{(result ? "PASS" : "FAIL")}");
            return result;
        }

        public bool TaskResponse(IEntityTaskWorker Worker, EntityTask Task)
        { return true; }

        public override string ToString()
        {
            return $"[{this.Height}::{this._onPlacement}]";
        }
    }
}
