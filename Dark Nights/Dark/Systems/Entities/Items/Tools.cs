using System.Collections;
using System.Collections.Generic;

namespace Dark.Entities
{
    [EntityDef]
    public class Tool_Hammer : EntityBase
    {
        public override string DefName => "entity.tool.hammer";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.FLOOR;

        public Tool_Hammer()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Item(1)
            };
            EntityGraphicsDef = new EntityGraphics(
                "entity.item.hammer",
                "basic",
                DefName,
                EntityLayer.OBJECTS);

        }
    }
}
