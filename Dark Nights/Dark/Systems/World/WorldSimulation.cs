using System.Collections;
using System.Collections.Generic;
using Nebula;

namespace Dark
{
    public interface IWorldTick
    {
        void WorldTick(float delta);
    }

    public class WorldSimulation
    {
        private static WorldSimulation instance;
        public static WorldSimulation Get => instance;
        List<IWorldTick> Simulated;

        public WorldSimulation()
        {
            instance = this;
        }

        public void Tick()
        {
            /*float delta = Time.deltaTime;
            foreach (var obj in Simulated)
            {
                obj.WorldTick(delta);
            }*/
        }

        public void Simulate(IWorldTick obj)
        {
            if (Simulated == null) Simulated = new List<IWorldTick>() { obj };
            else { Simulated.Add(obj); }
        }
    }
}
