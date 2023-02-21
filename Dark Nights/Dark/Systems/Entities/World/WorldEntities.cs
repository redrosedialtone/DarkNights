using System;
using System.Collections;
using System.Collections.Generic;

namespace Dark.Entities
{
    [EntityDef]
    public class ConcreteWall : EntityBase
    {
        public override string DefName => "entity.wall";
        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.CONSTRUCT;

        public ConcreteWall()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Impassable(),
                new Trait_HasMaterial(new BasicWallMaterialDef()),
            };

            EntityGraphicsDef = new EntityGraphics(
                "entity.wall-full",
                "basic",
                DefName,
                EntityLayer.CONSTRUCTS);

            //EntityGraphicsDef.AddTextureRules(new TextureAdjacentSelectionRule[]
            //{
            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-single",
            //       TextureAdjacentSelectionRule.TransformRule.Fixed,
            //       new int[]
            //       {
            //             0, -1,  0,
            //            -1,     -1,
            //             0, -1,  0
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-straight",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //             0, 1,  0,
            //            -1,    -1,
            //             0, 1,  0
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-end",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //            -1, -1, -1,
            //            -1,     -1,
            //             0,  1,  0
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-edge",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //             0, -1,  0,
            //             1,      1,
            //             1,  1,  1
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-full",
            //       TextureAdjacentSelectionRule.TransformRule.Fixed,
            //       new int[]
            //       {
            //             1, 1, 1,
            //             1,    1,
            //             1, 1, 1
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-trijunction",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //             -1,  1, -1,
            //              1,      1,
            //              0, -1,  0
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-trijunction-full",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //              1,  1, -1,
            //              1,      1,
            //             -1, -1, -1
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-quadjunction",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //             -1,  1,  -1,
            //              1,       1,
            //             -1,  1,  -1
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-edgejunction",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //             -1,  1,  -1,
            //              1,       1,
            //              1,  1,   1
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-corner",
            //       TextureAdjacentSelectionRule.TransformRule.Rotated,
            //       new int[]
            //       {
            //              0,  -1,   0,
            //              1,       -1,
            //             -1,   1,   0
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     ),

            //    new TextureAdjacentSelectionRule
            //    (
            //       this,
            //       "entity.wall-full-corner",
            //       TextureAdjacentSelectionRule.TransformRule.RMirrorX,
            //       new int[]
            //       {
            //              0,  -1,   0,
            //              1,       -1,
            //              1,   1,   0
            //       },
            //       TextureAdjacentSelectionRule.MatchRule.Layer
            //     )
            //});
        }
    }

    [EntityDef]
    public class BasicDoor : EntityBase
    {
        public override string DefName => "entity.door";
        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.CONSTRUCT;

        public BasicDoor()
        {
            Traits = new IEntityTrait[]
            {

            };

            EntityGraphicsDef = new EntityGraphics(
                "entity.door",
                "basic",
                DefName,
                EntityLayer.CONSTRUCTS);

            EntityGraphicsDef.AddTextureRules(new TextureAdjacentSelectionRule[]
            {
                new TextureAdjacentSelectionRule
                (
                   this,
                   "entity.door",
                   TextureAdjacentSelectionRule.TransformRule.Rotated,
                   new int[]
                   {
                         0, -1,  0,
                         1,      0,
                         0, -1,  0
                   },
                   TextureAdjacentSelectionRule.MatchRule.Layer
                 )
            });
        }
    }

    [EntityDef]
    public class BasicFloor : EntityBase
    {
        public override string DefName => "entity.basicfloor";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.FLOOR;

        public BasicFloor()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Ground(DefName)
            };
            EntityGraphicsDef = new EntityGraphics(
                "entity.basicFloor",
                "basic",
                DefName,
                EntityLayer.TILE);

        }
    }

    [EntityDef]
    public class Shrub : EntityBase
    {
        public override string DefName => "entity.shrub";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.FURNITURE;

        public Shrub()
        {
            Traits = new IEntityTrait[]
            {
                
            };
            int num = 0;// REFACTOR:Random.Next(0, 2);
            EntityGraphicsDef = new EntityGraphics(
                "entity.shrub" + num,
                "basic",
                DefName,
                EntityLayer.TILEDETAIL);
        }
    }

    [EntityDef]
    public class Tree : EntityBase
    {
        public override string DefName => "entity.tree";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.CONSTRUCT;

        public Tree()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Visibility(1),
                new Trait_Impassable()
            };

            EntityGraphicsDef = new EntityGraphics(
                "entity.tree",
                "basic",
                DefName,
                EntityLayer.CONSTRUCTS);
        }
    }

    [EntityDef]
    public class ConcreteFloor : EntityBase
    {
        public override string DefName => "entity.concretefloor";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.FLOOR;

        public ConcreteFloor()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Ground(DefName)
            };
            EntityGraphicsDef = new EntityGraphics(
                "entity.concretefloor",
                "simpleNoise",
                DefName,
                EntityLayer.TILE);

        }
    }

    [EntityDef]
    public class DirtFloor : EntityBase
    {
        public override string DefName => "entity.dirtfloor";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.FLOOR;

        public DirtFloor()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Ground(DefName)
            };
            EntityGraphicsDef = new EntityGraphics(
                "entity.dirtfloor",
                "simpleNoise",
                DefName,
                EntityLayer.TILE);

        }
    }

    [EntityDef]
    public class SandFloor : EntityBase
    {
        public override string DefName => "entity.sandfloor";

        public override IEntityTrait[] Traits { get; }
        public override EntityType EntityType => EntityType.FLOOR;

        public SandFloor()
        {
            Traits = new IEntityTrait[]
            {
                new Trait_Ground(DefName)
            };
            EntityGraphicsDef = new EntityGraphics(
                "entity.sandfloor",
                "simpleNoise",
                DefName,
                EntityLayer.TILE);

        }
    }
}
