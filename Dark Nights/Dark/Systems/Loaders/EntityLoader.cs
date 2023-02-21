using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Dark.Entities;

namespace Dark.Entities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class EntityDef : Attribute { }
}

namespace Dark.Systems
{
    public class EntityLoader
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[LOADINGSYS]");

        public Dictionary<string, IEntity> LoadComponentsFromAssembly()
        {
            log.Info("Loading Entities From Assembly...");
            IEnumerable<Type> _defs = FindClassTypes<EntityDef>(Assembly.GetExecutingAssembly());
            Dictionary<string, IEntity> ret = new Dictionary<string, IEntity>();

            foreach (var def in _defs)
            {
                IEntity entity = (IEntity)Activator.CreateInstance(def, new object[] { });
                ret.Add(entity.DefName, entity);
                log.Debug($"Loaded Entity::{entity.DefName}");
            }
            return ret;
        }

        private IEnumerable<Type> FindClassTypes<T>(Assembly assembly)
        {
            foreach (Type Def in assembly.GetTypes())
            {
                if (Def.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return Def;
                }
            }
        }
    }
}
