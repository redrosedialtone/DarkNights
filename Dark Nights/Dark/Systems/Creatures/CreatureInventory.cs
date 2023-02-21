using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public interface ICreatureInventory
    {
        IEntity[] InventoryContents { get; }
        int Encumberance { get; }

        bool AddItem(IEntity Item);
        bool RemoveItem(IEntity Item);
    }
}

namespace Dark.Creatures
{
    public abstract class CreatureInventory : ICreatureInventory
    {
        public IEntity[] InventoryContents { get; }
        private Dictionary<IEntity, ITraitContainable> contents = new Dictionary<IEntity, ITraitContainable>();
        public int Encumberance { get; }

        public virtual bool AddItem(IEntity Item)
        {
            var _findTrait = Item.FindTrait<ITraitContainable>();
            if (_findTrait != null && _findTrait is ITraitContainable containable)
            {
                AddContainable(Item, containable);
                return true;
            }
            return false;
        }

        protected virtual void AddContainable(IEntity EntityData, ITraitContainable Containable)
        {
            contents.Add(EntityData, Containable);

        }

        public virtual bool RemoveItem(IEntity Item)
        {
            if (contents.ContainsKey(Item))
            {
                return contents.Remove(Item);
            }
            return false;
        }
    }

    public class TestInventory : CreatureInventory
    {

    }
}
