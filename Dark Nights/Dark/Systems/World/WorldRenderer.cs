using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Nebula;
using Dark.World;
using Dark.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Dark
{
/* [0]BASE
 * [1-2]TILE - Water, Dirt, Floors, Grass
 * [3-4]FURNITURE - Chairs, Tables, Lamps
 * [5-6]OBJECTS - Entities, Food, Containers
 * [7-8]CHARACTERS - Players, Mobs, Wounds
 * [9-10]CONSTRUCTS - Walls, Windows, Trees, Buildings
 * [11]ABOVE - Tree Foliage, Rooves, Flying Creatures
 * [12]SKY - Clouds, Sunlight
 * 
 * SHADOWS - Rendered at layer; IE. Characters cast shadows 
 * over a table, but not over a wall.
 */
    public enum EntityLayer
    {
        BASE = 0,
        TILE = 1,
        TILEDETAIL = TILE + 1,
        FURNITURE = TILEDETAIL + 1,
        FURNITUREDETAIL = FURNITURE + 1,
        OBJECTS = FURNITUREDETAIL + 1,
        OBJECTDETAIL = OBJECTS + 1,
        CHARACTERS = FURNITUREDETAIL + 1,
        CHARACTERDETAIL = CHARACTERS + 1,
        CONSTRUCTS = CHARACTERDETAIL + 1,
        CONSTRUCTDETAIL = CONSTRUCTS + 1,
        ABOVE = CONSTRUCTDETAIL + 1,
        SKY = ABOVE + 1
    }
}

namespace Dark.Rendering
{
    public class WorldRenderer : IEntityTaskManager
    {
        #region Static
        private static WorldRenderer instance;
        public static WorldRenderer Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[RENDERER]");
        #endregion

        public WorldRenderer()
        {
            instance = this;
        }
        public static bool VISION_ENABLED = true;
        private readonly string UNEXPLORED_TEXTURE_ID = "unexplored";
        public Texture2D unseenTexture;
        public readonly string UNSEEN_MATERIAL_ID = "unseen";
        //private readonly Dictionary<ChunkLocation, RenderedChunk> RenderedChunks = new Dictionary<ChunkLocation, RenderedChunk>();
        //private readonly Dictionary<EntityID, RenderedEntity> RenderedEntities = new Dictionary<EntityID, RenderedEntity>();
        private readonly List<IRenderObject> renderObjects_dirty = new List<IRenderObject>();
        private readonly List<IRenderObject> renderMeshQueue = new List<IRenderObject>();

        public void Init()
        {
            
        }

        public void Tick()
        {
            RenderDirty();
            RenderMeshes();
        }

        private void RenderDirty()
        {
            IRenderObject[] renderQueue = renderObjects_dirty.ToArray();
            renderObjects_dirty.Clear();
            if (renderQueue != null)
            {
                foreach (var dirtyObj in renderQueue)
                {
                    if (dirtyObj == null) continue;
                    dirtyObj.RenderDirty();
                }
            }            
        }

        private void RenderMeshes()
        {
            foreach (var RenderObject in renderMeshQueue)
            {
                RenderObject.RenderMeshes();
            }
        }

        public void RenderObjectDestroyed(IRenderObject RenderedObject)
        {
            if (renderMeshQueue.Contains(RenderedObject))
            {
                renderMeshQueue.Remove(RenderedObject);
            }
            if (renderObjects_dirty.Contains(RenderedObject))
            {
                renderObjects_dirty.Remove(RenderedObject);
            }
        }

        /*public RenderedChunk RenderNewChunk(IWorldChunk ChunkData)
        {
            RenderedChunk _chunk = new RenderedChunk(TileMapTransform, ChunkData);
            RenderedChunks.Add(ChunkData.Coordinates, _chunk);
            _chunk.SetDirty();
            return _chunk;
        }*/

        public void RenderObject_SetDirty(IRenderObject RenderObject)
        {
            log.Debug($"RenderObject Set Dirty::{RenderObject.Coordinates}");
            if(!renderObjects_dirty.Contains(RenderObject)) renderObjects_dirty.Add(RenderObject);
        }

        public void RenderObject_AddToMeshQueue(IRenderObject RenderObject)
        {
            if (!renderMeshQueue.Contains(RenderObject)) { renderMeshQueue.Add(RenderObject); }
        }

        public bool TaskResponse(IEntityTaskWorker Worker, EntityTask Task)
        { return true; }

        public static void InvalidateEntity(IEntity Entity)
        {
            //SetEntityDirty(Entity);
        }

        #region Helper Functions

        /*public static void SetChunkDirty(RenderedChunk Chunk) 
        { instance.RenderObject_SetDirty(Chunk); }

        // Set Tile Dirty
        public static void SetTileDirty(ITileData Data)
        { instance.instance_SetTileDirty(Data.Coordinates); }

        public static void SetTileDirty(WorldPoint Coordinates)
        { instance.instance_SetTileDirty(Coordinates); }

        public static void SetTileDirty(RenderedTile RenderObject)
        { instance.RenderObject_SetDirty(RenderObject); }

        private void instance_SetTileDirty(WorldPoint Coordinates)
        {  RenderObject_SetDirty(RenderedChunks[Coordinates].Tile(Coordinates)); }

        // Set Entity Dirty
        public static void SetEntityDirty(IEntity EntityData)
        { instance.instance_SetEntityDirty(EntityData.ID); }

        public static void SetEntityDirty(RenderedEntity RenderedObject)
        { instance.RenderObject_SetDirty(RenderedObject); }

        public static void SetEntityDirty(EntityID ID)
        { instance.instance_SetEntityDirty(ID); }

        private void instance_SetEntityDirty(EntityID ID)
        { RenderObject_SetDirty(GetRenderedEntity(ID)); }

        // Get Chunk Data

        private IWorldChunk GetChunkData(WorldPoint Coordinates, out CbTileState cbTileState)
        { return WorldSystem.Chunk(Coordinates, out cbTileState); }

        public IWorldChunk GetChunkData(RenderedChunk RenderObject, out CbTileState cbTileState)
        { return WorldSystem.Chunk(RenderObject.Coordinates, out cbTileState); }

        private RenderedChunk GetRenderedChunk(IWorldChunk ChunkData)
        { return RenderedChunks[ChunkData.Coordinates]; }

        // Get Tile Data

        private ITileData GetTileData(WorldPoint Coordinates, out CbTileState cbTileState)
        { return WorldSystem.Tile(Coordinates, out cbTileState); }

        public ITileData GetTileData(RenderedTile RenderObject, out CbTileState cbTileState)
        { return WorldSystem.Tile(RenderObject.Coordinates, out cbTileState); }

        private RenderedTile GetRenderedTile(ITileData TileData)
        { return RenderedChunks[TileData.Coordinates].Tile(TileData.Coordinates); }

        public RenderedTile GetRenderedTile(WorldPoint Coordinates)
        {
            if (RenderedChunks.ContainsKey(Coordinates))
            {
                RenderedChunk ChunkData = RenderedChunks[Coordinates];
                if (ChunkData != null) return ChunkData.Tile(Coordinates);
            }
            return null;
        }

        // Get Entity Data

        public void NewRenderedEntity(EntityID ID, RenderedEntity newEntity)
        {
            if (RenderedEntities.ContainsKey(ID))
            {
                this.Warning("Entity Key Already Exists::"+ID);
            }
            else
            {
                RenderedEntities.Add(ID, newEntity);
            }
            
        }

        private IEntity GetEntityData(WorldPoint Coordinates, EntityID ID, out CbTileState cbTileState)
        {
            var _tile = GetTileData(Coordinates, out cbTileState);
            if (cbTileState != CbTileState.OK) return null;
            return _tile.Container.GetEntity(ID);
        }

        private IEntity GetEntityData(RenderedEntity RenderObject, out CbTileState cbTileState)
        {
            var _tile = GetTileData(RenderObject.Coordinates, out cbTileState);
            if (cbTileState != CbTileState.OK) return null;
            return _tile.Container.GetEntity(RenderObject.ID); 
        }

        public RenderedEntity GetRenderedEntity(EntityID ID) 
        { if (RenderedEntities.ContainsKey(ID)) return RenderedEntities[ID];
            else this.Verbose("Entity Not Found::" + ID); return null;
        }


        public IEnumerable<RenderedEntity> GetRenderedEntities()
        {
            foreach (var kv in RenderedEntities)
            {
                yield return kv.Value;
            }
        }*/
        #endregion
    }

    public interface IRenderObject
    {
        WorldPoint Coordinates { get; }
        /// <summary>
        /// Set for Render
        /// </summary>
        void SetDirty();
        /// <summary>
        /// Re-Render the item
        /// </summary>
        void RenderDirty();
        /// <summary>
        /// Render the relevant meshes
        /// </summary>
        void RenderMeshes();
        bool Rendering { get; }

        void OnRenderUpdate(Action<IRenderObject> act);
        void CancelOnRenderUpdate(Action<IRenderObject> act);
    }
/*
    public class RenderedChunk : IRenderObject
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[RENDERER]");
        public string LoggingPrefix => $"<color=green>[RCHUNK:{Object.name}]</color>";

        public RenderedTile[,] RenderedTiles;
        public IEnumerable<RenderedTile> GetRenderedTiles() {
            for (int x = 0; x < RenderedTiles.GetLength(0); x++) {
                for (int y = 0; y < RenderedTiles.GetLength(1); y++) {
                    yield return RenderedTiles[x, y];
                }
            }
        }
        public WorldPoint Coordinates { get => coordinates; }
        private readonly ChunkLocation coordinates;
        public bool Rendering { get; } = true;

        private Action<IRenderObject> renderUpdateEvent;

        public RenderedChunk(IWorldChunk ChunkData)
        {
            WorldRenderer.Get.RenderObject_AddToMeshQueue(this);

            this.coordinates = ChunkData.Coordinates;

            log.Trace($"Created Render Chunk::{ChunkData.Coordinates}");

            InitialiseRenderedTiles();
        }

        private void InitialiseRenderedTiles()
        {
            log.Trace($"Creating Rendered Chunk from {Coordinates} of size {WorldSystem.CHUNK_SIZE}");
            RenderedTiles = new RenderedTile[WorldSystem.CHUNK_SIZE, WorldSystem.CHUNK_SIZE];
            for (int x = 0; x < RenderedTiles.GetLength(0); x++) {
                for (int y = 0; y < RenderedTiles.GetLength(1); y++) {
                    RenderedTiles[x, y] = new RenderedTile(new WorldPoint(Coordinates.X + x, Coordinates.Y + y));
                }
            }
        }

        public RenderedTile Tile(WorldPoint Point)
        {
            int _x = (int)MathF.Abs(Coordinates.X - Point.X);
            int _y = (int)MathF.Abs(Coordinates.Y - Point.Y);
            return RenderedTiles[_x,_y];
        }

        public void RenderMeshes()
        {
            if (Rendering)
            {
                foreach (var RenderedTile in GetRenderedTiles())
                {
                    RenderedTile.RenderMeshes();                    
                }
            }            
        }

        public void RenderDirty() { renderUpdateEvent?.Invoke(this); }

        public void SetDirty()
        {
            foreach (var RenderedTile in GetRenderedTiles())
            {
                WorldRenderer.SetTileDirty(RenderedTile);
            }
            WorldRenderer.SetChunkDirty(this);
        }

        public void OnRenderUpdate(Action<IRenderObject> act)
        {
            renderUpdateEvent += act;
        }

        public void CancelOnRenderUpdate(Action<IRenderObject> act)
        {
            renderUpdateEvent -= act;
        }
    }

    public class RenderedTile : IRenderObject, IEntityTaskManager
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[RENDERER]");
        public List<EntityID> RenderedEntities = new List<EntityID>();
        
        public WorldPoint Coordinates { get => coordinates; }
        private readonly WorldPoint coordinates;
        public bool Rendering { get; } = true;
        private Visibility tileVisibility;
        private bool hidden => tileVisibility == Visibility.Hidden;

        private Action<IRenderObject> renderUpdateEvent;
        private int tileContentsHash;

        private MeshData hiddenMesh;

        public RenderedTile(WorldPoint Coordinates)
        {
            this.coordinates = Coordinates;
        }

        public void SetDirty()
        {
            foreach (var ID in RenderedEntities)
            {
                WorldRenderer.SetEntityDirty(ID);
            }
            WorldRenderer.SetTileDirty(this);
        }

        public void RenderDirty()
        {
            //this.Debug($"Rendering Tile {Coordinates}", LoggingPriority.High);
            renderUpdateEvent?.Invoke(this);
            ITileData Data = WorldRenderer.Get.GetTileData(this, out cbTileState cbTileState);
            if(cbTileState != cbTileState.OK)
            {
                log.Warn($"Tile Data {Coordinates} missing? ({cbTileState})");
                return;
            }

            // Update the tile name
            string _name = "Unknown Tile";
            tileVisibility = Data.VisibilityData.PlayerVisibility;
            if (tileVisibility == Visibility.Hidden && WorldRenderer.VISION_ENABLED)
            {
                if (hiddenMesh == null)
                {
                    if (hiddenMesh == null) hiddenMesh = new MeshData(1, MeshFlags.UV);
                    else hiddenMesh.Clear();

                    int vIndex = hiddenMesh.vertices.Count;

                    hiddenMesh.vertices.Add(new Vector3(0, 0));
                    hiddenMesh.vertices.Add(new Vector3(0, 1));
                    hiddenMesh.vertices.Add(new Vector3(1, 1));
                    hiddenMesh.vertices.Add(new Vector3(1, 0));

                    hiddenMesh.UVs.Add(new Vector3(0, 0));
                    hiddenMesh.UVs.Add(new Vector2(0, 1));
                    hiddenMesh.UVs.Add(new Vector2(1, 1));
                    hiddenMesh.UVs.Add(new Vector2(1, 0));

                    hiddenMesh.AddTriangle(vIndex, 0, 1, 2);
                    hiddenMesh.AddTriangle(vIndex, 0, 2, 3);

                    hiddenMesh.Build();
                }
            }
            else
            {
                if (tileVisibility == Visibility.Seen && WorldRenderer.VISION_ENABLED)
                {
                    if (hiddenMesh == null)
                    {
                        if (hiddenMesh == null) hiddenMesh = new MeshData(1, MeshFlags.UV);
                        else hiddenMesh.Clear();

                        int vIndex = hiddenMesh.vertices.Count;

                        hiddenMesh.vertices.Add(new Vector3(0, 0));
                        hiddenMesh.vertices.Add(new Vector3(0, 1));
                        hiddenMesh.vertices.Add(new Vector3(1, 1));
                        hiddenMesh.vertices.Add(new Vector3(1, 0));

                        hiddenMesh.UVs.Add(new Vector3(0, 0));
                        hiddenMesh.UVs.Add(new Vector2(0, 1));
                        hiddenMesh.UVs.Add(new Vector2(1, 1));
                        hiddenMesh.UVs.Add(new Vector2(1, 0));

                        hiddenMesh.AddTriangle(vIndex, 0, 1, 2);
                        hiddenMesh.AddTriangle(vIndex, 0, 2, 3);

                        hiddenMesh.Build();
                    }
                }
                // Update the tile's entities
                if (RenderedEntities != null)
                {
                    int p_tileContentsHash = tileContentsHash;
                    tileContentsHash = Data.Container.GetHashCode();
                    //Debug.Log($"Tile Hash Code: {p_tileContentsHash} / {tileContentsHash}");

                    // If the list of entities has changed
                    if (tileContentsHash != p_tileContentsHash)
                    {
                        log.Trace($"Rendering Tile Entities [{RenderedEntities.Count}]");
                        var _renderedEntities = RenderedEntities.ToArray();
                        foreach (var EntityID in _renderedEntities)
                        {
                            // If a rendered entity has been removed
                            if (!Data.Container.HasEntity(EntityID))
                            {
                                var _entity = WorldRenderer.Get.GetRenderedEntity(EntityID);
                                if (_entity != null)
                                {
                                    _entity.Destroyed();
                                }
                                RenderedEntities.Remove(EntityID);
                            }
                        }

                        foreach (IEntity entity in Data.Container.TileEntities())
                        {
                            // If there's a new entity to render
                            if (!RenderedEntities.Contains(entity.ID))
                            {
                                RenderedEntity renderedEntity = new RenderedEntity(this, entity);
                                RenderedEntities.Add(entity.ID);
                                WorldRenderer.Get.NewRenderedEntity(entity.ID, renderedEntity);

                            }
                        }
                        SetDirty();
                    }
                }
            }      
        }

        public RenderedEntity GetRenderedEntity(IEntity EntityData)
        { return GetRenderedEntity(EntityData.ID); }

        public RenderedEntity GetRenderedEntity(EntityID ID)
        { return WorldRenderer.Get.GetRenderedEntity(ID); }

        public bool TaskResponse(IEntityTaskWorker Worker, EntityTask Task)
        { return true; }

        public void RenderMeshes()
        {
            if (Rendering)
            {
                if (hidden && WorldRenderer.VISION_ENABLED)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    block.SetColor("Tint",Color.black);
                    Graphics.DrawMesh(
                        hiddenMesh.mesh,
                        Object.transform.position,
                        Quaternion.identity,
                        WorldRenderer.Get.unexploredMat,
                        0
                     );
                }
                else
                {
                    /*if (tileVisibility == Visibility.Seen)
                    {
                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        block.SetColor("Tint", new Color(0.2f,0.2f,0.2f,0.5f));
                        Graphics.DrawMesh(
                            hiddenMesh.mesh,
                            Object.transform.position,
                            Quaternion.identity,
                            WorldRenderer.Get.unexploredMat,
                            0
                         );
                    }
                    foreach (var EntityID in RenderedEntities)
                    {
                        var _entity = WorldRenderer.Get.GetRenderedEntity(EntityID);
                        if (_entity != null)
                        {
                            _entity.Visibility = tileVisibility;
                            _entity.RenderMeshes();
                        }
                    }
                }
            }           
        }

        public void OnRenderUpdate(Action<IRenderObject> act)
        {
            renderUpdateEvent += act;
        }

        public void CancelOnRenderUpdate(Action<IRenderObject> act)
        {
            renderUpdateEvent -= act;
        }
    }

    public class RenderedEntity : IRenderObject, IEntityTaskManager
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[RENDERER]");

        public EntityID ID;
        public WorldPoint Coordinates { get => coordinates; }
        private readonly WorldPoint coordinates;
        public bool Rendering { get; private set; } = true;
        public Visibility Visibility;

        public Material Material;
        public Texture2D Texture;
        public Color Color;
        public IEntityGraphics GraphicsDef;

        private Material unseenMat;


        private Action<IRenderObject> renderUpdateEvent;
        private readonly RenderedTile Tile;
        private MeshData meshData;
        private EntityTextureSettings CurrentTextureSettings;
        private bool[] readingFromNeighbours;

        private bool _destroyed;

        public RenderedEntity(RenderedTile Tile, IEntity Data)
        {

            this.ID = Data.ID;
            this.Tile = Tile;
            this.coordinates = Tile.Coordinates;

            this.Debug($"Render Object Creation::{Data.DefName}{Tile.Coordinates}(#{Data.ID})", LoggingPriority.High);

            if (Data.EntityGraphicsDef != null)
            {
                GraphicsDef = Data.EntityGraphicsDef;
                this.Material = new Material(ResourceManager.LoadEntityMaterial(GraphicsDef.MaterialID));
                _go.transform.localPosition = new Vector3(_go.transform.localPosition.x, _go.transform.localPosition.y, -(int)GraphicsDef.Layer);
            }

            unseenMat = new Material(ResourceManager.LoadEntityMaterial(WorldRenderer.Get.UNSEEN_MATERIAL_ID));
        }

        public void SetDirty()
        { WorldRenderer.SetEntityDirty(this); }

        public void RenderDirty()
        {
            if (_destroyed) return;
            this.Debug($"Rendering Dirty {Object.name}");
            renderUpdateEvent?.Invoke(this);
            BuildMesh();
        }

        public void RenderMeshes()
        {
            if (Rendering)
            {
                if (meshData != null)
                {
                    if (Visibility != Visibility.Visible && WorldRenderer.VISION_ENABLED)
                    {
                        Graphics.DrawMesh(
                        meshData.mesh,
                        Object.transform.position,
                        Quaternion.identity,
                        unseenMat,
                        0
                        );
                        
                    }
                    else
                    {
                        Graphics.DrawMesh(
                        meshData.mesh,
                        Object.transform.position,
                        Quaternion.identity,
                        this.Material,
                        0
                        );
                    }
                    
                }
                
            }           
        }

        private void BuildMesh()
        {
            if (GraphicsDef != null)
            {
                EntityTextureSettings TextureSettings = GraphicsDef.GetTexture(WorldRenderer.Get.GetTileData(Tile, out cbTileState cbTileState));
                if(cbTileState != cbTileState.OK) { this.Warning($"Failed building mesh!! ({cbTileState})"); return; }
                if (TextureSettings != null && TextureSettings != CurrentTextureSettings)
                {
                    this.Debug($"Building Entity Mesh {Object.name}::Texture::({TextureSettings.TextureID})");

                    CurrentTextureSettings = TextureSettings;

                    if (readingFromNeighbours != TextureSettings.ReadFromNeighbours)
                    {
                        if (TextureSettings.ReadFromNeighbours != null)
                        {
                            if (readingFromNeighbours != null)
                            {
                                for (int i = 0; i < readingFromNeighbours.Length; i++)
                                {
                                    // If we need to subscribe
                                    if (!readingFromNeighbours[i] && TextureSettings.ReadFromNeighbours[i])
                                    {
                                        Vector2Int neighbourPos = AdjacentTileData.ToCoordinate[i];
                                        RenderedTile Neighbour = WorldRenderer.Get.GetRenderedTile(
                                            new WorldPoint(Coordinates.X + neighbourPos.x, Coordinates.Y + neighbourPos.y));
                                        if (Neighbour != null)
                                        {
                                            this.Debug($"Adding Neighbour Render Update {Coordinates}::{neighbourPos}", LoggingPriority.Low);
                                            Neighbour.OnRenderUpdate(x => SetDirty());
                                        }
                                    }
                                    // If we need to unsubscribe 
                                    else if (!TextureSettings.ReadFromNeighbours[i] && readingFromNeighbours[i])
                                    {
                                        Vector2Int neighbourPos = AdjacentTileData.ToCoordinate[i];
                                        RenderedTile Neighbour = WorldRenderer.Get.GetRenderedTile(
                                            new WorldPoint(Coordinates.X + neighbourPos.x, Coordinates.Y + neighbourPos.y));
                                        if (Neighbour != null)
                                        {
                                            this.Debug($"Removing Neighbour Render Update {Coordinates}::{neighbourPos}", LoggingPriority.Low);
                                            Neighbour.CancelOnRenderUpdate(x => SetDirty());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                readingFromNeighbours = TextureSettings.ReadFromNeighbours;
                                for (int i = 0; i < readingFromNeighbours.Length; i++)
                                {
                                    if (readingFromNeighbours[i])
                                    {
                                        Vector2Int neighbourPos = AdjacentTileData.ToCoordinate[i];
                                        RenderedTile Neighbour = WorldRenderer.Get.GetRenderedTile(
                                            new WorldPoint(Coordinates.X + neighbourPos.x, Coordinates.Y + neighbourPos.y));
                                        if (Neighbour != null)
                                        {
                                            this.Debug($"Adding Neighbour Render Update {Neighbour.Coordinates}::{AdjacentTileData.IndexToString[i]}", LoggingPriority.Low);
                                            Neighbour.OnRenderUpdate(x => SetDirty());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.Debug($"Clearing Neighbour Render Update {Coordinates}");
                            for (int i = 0; i < readingFromNeighbours.Length; i++)
                            {
                                if (readingFromNeighbours[i])
                                {
                                    Vector2Int neighbourPos = AdjacentTileData.ToCoordinate[i];
                                    RenderedTile Neighbour = WorldRenderer.Get.GetRenderedTile(
                                        new WorldPoint(Coordinates.X + neighbourPos.x, Coordinates.Y + neighbourPos.y));
                                    if (Neighbour != null)
                                    {
                                        this.Debug($"Removing Neighbour Render Update {Coordinates}::{neighbourPos}", LoggingPriority.Low);
                                        Neighbour.CancelOnRenderUpdate(x => SetDirty());
                                    }
                                }

                            }
                        }
                    }

                    this.Texture = ResourceManager.LoadEntityTexture(TextureSettings.TextureID);
                    this.Material.mainTexture = Texture;
                    this.Material.SetTexture("_MainTex", Texture);
                    unseenMat.SetTexture("_MainTex", Texture);
       
                    if (meshData == null) meshData = new MeshData(1, MeshFlags.UV);
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
                    float rads = Mathf.Deg2Rad * TextureSettings.Angle;
                    float rotMatrix00 = Mathf.Cos(rads);
                    float rotMatrix01 = -Mathf.Sin(rads);
                    float rotMatrix10 = Mathf.Sin(rads);
                    float rotMatrix11 = Mathf.Cos(rads);

                    if (TextureSettings.MirrorX)
                    {
                        for (int i = 0; i < meshData.UVs.Count; i++)
                        {
                            Vector2 meshPivot = meshData.UVs[i] - pivot;
                            float x = rotMatrix00 * meshPivot.x + rotMatrix01 * meshPivot.y;
                            float y = rotMatrix10 * meshPivot.x + rotMatrix11 * meshPivot.y;
                            meshData.UVs[i] = new Vector2(y,x) + pivot;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < meshData.UVs.Count; i++)
                        {
                            Vector2 meshPivot = meshData.UVs[i] - pivot;
                            float x = rotMatrix00 * meshPivot.x + rotMatrix01 * meshPivot.y;
                            float y = rotMatrix10 * meshPivot.x + rotMatrix11 * meshPivot.y;
                            meshData.UVs[i] = new Vector2(x, y) + pivot;
                        }
                    }                   

                    meshData.AddTriangle(vIndex, 0, 1, 2);
                    meshData.AddTriangle(vIndex, 0, 2, 3);

                    meshData.Build();
                }
            }
        }

        public void Destroyed()
        {
            WorldRenderer.Get.RenderObjectDestroyed(this);
            GameObject.Destroy(Object);
            _destroyed = true;
            Rendering = false;

            
            if (readingFromNeighbours != null)
            {
                this.Debug($"Clearing Neighbour Render Update {Coordinates}");
                for (int i = 0; i < readingFromNeighbours.Length; i++)
                {
                    if (readingFromNeighbours[i])
                    {
                        Vector2Int neighbourPos = AdjacentTileData.ToCoordinate[i];
                        RenderedTile Neighbour = WorldRenderer.Get.GetRenderedTile(
                            new WorldPoint(Coordinates.X + neighbourPos.x, Coordinates.Y + neighbourPos.y));
                        if (Neighbour != null)
                        {
                            this.Debug($"Removing Neighbour Render Update {Coordinates}::{neighbourPos}", LoggingPriority.Low);
                            Neighbour.CancelOnRenderUpdate(x => SetDirty());
                        }
                    }

                }
            }
            
            renderUpdateEvent = null;
        }

        public void OnRenderUpdate(Action<IRenderObject> act)
        {
            renderUpdateEvent += act;
        }

        public void CancelOnRenderUpdate(Action<IRenderObject> act)
        {
            renderUpdateEvent -= act;
        }

        public bool TaskResponse(IEntityTaskWorker Worker, EntityTask Task)
        { return true; }
    }*/
}
