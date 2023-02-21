using System.Collections.Generic;
using System;
using Nebula.Systems;
using Dark.Creatures;
using Nebula;
using Dark.Rendering;
using Microsoft.Xna.Framework;

namespace Dark
{
    public class CreatureController : IManager, IWorldTick
    {
        #region Static
        private static CreatureController instance;
        public static CreatureController Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[CREATURESYS]");
        #endregion

        private bool drawCharactersGizmo;

        private List<ICreature> simulatedCreatures;

        public static TestCreature TEST_CREATURE => (TestCreature)instance.simulatedCreatures[0];

        public bool Initialized => throw new NotImplementedException();

        public void Init()
        {
            log.Info("> Creature Controller Init.. <");
            instance = this;
        }

        public void OnInitialized()
        {
            WorldSimulation.Get.Simulate(this); 
        }

        public IPlayerCharacter CreatePlayerCharacter(WorldPoint Coordinate)
        {
            PlayerCharacter playerCharacter = new PlayerCharacter();
            playerCharacter.SetTilePosition(Coordinate);

            if (simulatedCreatures == null) simulatedCreatures = new List<ICreature>() { playerCharacter };
            else simulatedCreatures.Add(playerCharacter);

            TaskSystem.Worker(playerCharacter.AI);

            /*RenderedCreature renderObject = new RenderedCreature(playerCharacter);
            renderObjects_dirty.Add(renderObject);*/
            return playerCharacter;
        }

        public void WorldTick(float delta)
        {
            foreach (var creature in simulatedCreatures)
            {
                creature.WorldTick(delta);
            }
            RenderCreatures();
        }

        #region Rendering

        private readonly List<IRenderObject> renderObjects_dirty = new List<IRenderObject>();
        private readonly List<IRenderObject> renderMeshQueue = new List<IRenderObject>();

        public static void RenderMeshQueue(IRenderObject renderObject)
        {
            instance.renderMeshQueue.Add(renderObject);
        }

        public static void Dirty(IRenderObject renderObject)
        {
            instance.renderObjects_dirty.Add(renderObject);
        }

        private void RenderCreatures()
        {
            // Rebuild Dirty Render Objects
            IRenderObject[] renderQueue = renderObjects_dirty.ToArray();
            renderObjects_dirty.Clear();
            if (renderQueue != null)
            {
                foreach (var dirtyObj in renderQueue)
                {
                    dirtyObj.RenderDirty();
                }
            }

            // Render the meshes

            foreach (var renderObject in renderMeshQueue)
            {
                renderObject.RenderMeshes();
            }
        }

        #endregion


        #region Gizmos

        /*public void OnDrawGizmos()
        {
            if (Initialized)
            {
                if (simulatedCreatures != null)
                {
                    if (drawCharactersGizmo)
                    {
                        foreach (var creature in simulatedCreatures)
                        {
                            Gizmos.color = new Color(1f, 0.8f, 0.7f, 1f);
                            Vector3 renderPos = new Vector3(creature.Navigation.Position.x + 0.5f, creature.Navigation.Position.y + 0.5f);
                            Vector3 nosePosition = new Vector3(renderPos.x, renderPos.y + 0.2f);
                            Gizmos.DrawCube(renderPos, new Vector3(0.8f, 0.2f, 0.2f));
                            Gizmos.DrawCube(nosePosition, new Vector3(0.1f, 0.2f, 0.1f));

                            Gizmos.color = new Color(0.6f, 1, 0.25f, 1f);
                            Gizmos.DrawCube(renderPos, new Vector3(0.5f, 0.35f, 0.5f));

                            Gizmos.color = Color.red;
                            
                            Vector3 facingArrow = new Vector3(0.75f, 0.1f, 0.1f);
                            float angle = Mathf.Atan2(creature.RenderFacing.y, creature.RenderFacing.x) * Mathf.Rad2Deg;
                            Matrix4x4 rotationMatrix = Matrix4x4.TRS(renderPos, Quaternion.Euler(new Vector3(0,0,angle)), Vector3.one);
                            Gizmos.matrix = rotationMatrix;
                            Gizmos.DrawCube(Vector3.zero, facingArrow);

                        }
                    }
                }
            }
        }*/

        public void Tick()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public interface ICreatureRenderData
    {
        Vector2 RenderPoint { get; }
        Vector2 RenderFacing { get; }
        string RenderTexture { get; }
    }

 /*   public class RenderedCreature : IRenderObject
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[RENDEREDCREATURE]");

        public WorldPoint Coordinates { get; set; }
        public bool Rendering { get; } = true;

        private readonly ICreatureRenderData Data;

        private Material Material;
        private Color Color;

        private MeshData meshData;
        private Action<IRenderObject> renderUpdateEvent;

        public RenderedCreature(ICreatureRenderData Data)
        {
            this.Data = Data;
            this.Material = new Material(ResourceManager.LoadEntityMaterial("basic"));
            CreatureController.RenderMeshQueue(this);

            Texture2D Texture = ResourceManager.LoadCharacterTexture(Data.RenderTexture);
            this.Material.mainTexture = Texture;
        }

        public void SetDirty()
        { CreatureController.Dirty(this); }

        public void RenderDirty()
        {
            log.Trace($"Rendering Character..");
            renderUpdateEvent?.Invoke(this);
            BuildMesh();
        }

        public void OnRenderUpdate(Action<IRenderObject> act) { }
        public void CancelOnRenderUpdate(Action<IRenderObject> act) { }

        public void RenderMeshes()
        {
            if (Rendering)
            {
                BuildMesh();
                Quaternion rotation = Quaternion.identity;
                Vector3 position = new Vector3(Data.RenderPoint.x, Data.RenderPoint.y, -(int)EntityLayer.CHARACTERS);

                Graphics.DrawMesh(
                    meshData.mesh,
                    position,
                    rotation,
                    this.Material,
                    0
                 );
            }
        }

        private void BuildMesh()
        {
            if(meshData == null) meshData = new MeshData(1, MeshFlags.UV);
            else meshData.Clear();

            int vIndex = meshData.vertices.Count;

            meshData.vertices.Add(new Vector3(0, 0));
            meshData.vertices.Add(new Vector3(0, 1));
            meshData.vertices.Add(new Vector3(1, 1));
            meshData.vertices.Add(new Vector3(1, 0));

            meshData.UVs.Add(new Vector3(0, 0));
            meshData.UVs.Add(new Vector2(0, 1));
            meshData.UVs.Add(new Vector2(1, 1));
            meshData.UVs.Add(new Vector2(1, 0));

            Vector2 pivot = new Vector2(0.5f, 0.5f);
            float angle = Mathf.Atan2(Data.RenderFacing.x, Data.RenderFacing.y);
            float rotMatrix00 = Mathf.Cos(angle);
            float rotMatrix01 = -Mathf.Sin(angle);
            float rotMatrix10 = Mathf.Sin(angle);
            float rotMatrix11 = Mathf.Cos(angle);

            for (int i = 0; i < meshData.UVs.Count; i++)
            {
                Vector2 meshPivot = meshData.UVs[i] - pivot;
                float x = rotMatrix00 * meshPivot.x + rotMatrix01 * meshPivot.y;
                float y = rotMatrix10 * meshPivot.x + rotMatrix11 * meshPivot.y;
                meshData.UVs[i] = new Vector2(x, y) + pivot;
            }

            meshData.AddTriangle(vIndex, 0, 1, 2);
            meshData.AddTriangle(vIndex, 0, 2, 3);

            meshData.Build();
        }
    }*/
}