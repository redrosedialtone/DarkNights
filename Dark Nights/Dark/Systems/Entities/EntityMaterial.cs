using Dark.Entities;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public interface IEntityMaterialDef
    {
        IEntityMaterialType[] EntityMaterials { get; }
        float[] EntityMaterialComposition { get; }
    }
}

namespace Dark.Entities
{
    /// <summary>
    /// Placeholder for JSON definitions
    /// </summary>
    public static class ENTITY_DEF_TEMP
    {
        public static Dictionary<string, EntityMaterialBase> Materials = new Dictionary<string, EntityMaterialBase>()
        {
            {"metal", new Metal() },
            {"wood", new Wood() },
            {"concrete", new Concrete() }
        };
    }

    public abstract class EntityMaterialDef : IEntityMaterialDef
    {
        public abstract IEntityMaterialType[] EntityMaterials { get; }
        public abstract float[] EntityMaterialComposition { get; }
    }

    public class BasicWallMaterialDef : EntityMaterialDef
    {
        public override IEntityMaterialType[] EntityMaterials => new IEntityMaterialType[]
        {
            new Concrete(),
            new Metal()
        };

        public override float[] EntityMaterialComposition => new float[]
        {
            0.75f,
            0.25f
        };
    }

    public interface IEntityMaterialType
    {
        string MaterialName { get; }
    }

    public abstract class EntityMaterialBase : IEntityMaterialType
    {
        public abstract string MaterialName { get; }
        public abstract string MaterialDef { get; }
    }

    public class Metal : EntityMaterialBase
    {
        public override string MaterialName => "Metal";
        public override string MaterialDef => "metal";
    }

    public class Wood : EntityMaterialBase
    {
        public override string MaterialName => "Wood";
        public override string MaterialDef => "wood";
    }

    public class Concrete : EntityMaterialBase
    {
        public override string MaterialName => "Concrete";
        public override string MaterialDef => "concrete";
    }
}
