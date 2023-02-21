using Dark.Systems.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public interface IPlayerCharacter : ICreature
    {
        string Name { get; }
    }
}

namespace Dark.Creatures
{
    public class PlayerCharacter : CreatureBase, IPlayerCharacter
    {
        public string Name { get; }

        public IWorkOrder CurrentOrder { get; private set; }
        public LinkedList<IWorkOrder> TaskQueue { get; } = new LinkedList<IWorkOrder>();
        public bool Available { get; } = true;

        public IVision Vision => _sight;
        private readonly BaseCreatureSight _sight;
        public override ICreatureNavigation Navigation => _navigation;
        private readonly CreatureBaseNavigation _navigation;
        public override string RenderTexture => "character";

        public PlayerCharacter()
        {
            _navigation = new CreatureBaseNavigation(this);
            AI = new TestAI(this);
            Inventory = new TestInventory();
            _sight = new PlayerSight(this, 16);
            Name = "Clyde Survival";
        }

        public override void WorldTick(float delta)
        {
            base.WorldTick(delta);
            AI.Tick();
            _navigation.WorldTick(delta);
        }
    }
}
