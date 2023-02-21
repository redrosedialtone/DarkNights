using Dark.Systems;
using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class LoadingSystem : Manager
    {
        #region Static
        private static LoadingSystem instance;
        public static LoadingSystem Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("LOADING");
        #endregion

        public Dictionary<string, IEntity> EntityDefs = new Dictionary<string, IEntity>();

        public override void Init()
        {
            log.Info(" > Loading System Init.. <");
            instance = this;
            EntityLoader loader = new EntityLoader();
            EntityDefs = loader.LoadComponentsFromAssembly();
            base.Init();
        }
    }
}
