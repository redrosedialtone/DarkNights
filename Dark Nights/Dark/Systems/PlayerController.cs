using Dark.Systems.Tasks;
using Nebula;
using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class PlayerController : IManager
    {
        #region Static
        private static PlayerController instance;
        public static PlayerController Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[PLAYER]");
        public bool Initialized => throw new System.NotImplementedException();
        #endregion

        private const int PLAYER_WORLDGEN_RADIUS = 3;

        public ICreature selectedCreature;
        private List<IPlayerCharacter> PlayerCharacters = new List<IPlayerCharacter>();

        public void Init()
        {
            log.Info("> Player Controller Init <");
            instance = this;
        }

        public void OnInitialized()
        {
            //BuildSelectionOverlayMesh();

            PlayerCharacter(CreatureController.Get.CreatePlayerCharacter(new WorldPoint(3, 3)));
        }

        public void Tick()
        {
            //RenderCharacterSelector();
        }

        public void PlayerCharacter(IPlayerCharacter newCharacter)
        {
            PlayerCharacters.Add(newCharacter);
            newCharacter.Navigation.TraversedChunk((a,b) => PlayerCharacterTraversedChunk(newCharacter.Navigation,a,b));
        }

        public void PlayerCharacterTraversedChunk(ICreatureNavigation navigation, ChunkLocation from, ChunkLocation to)
        {
            log.Trace("OnPlayerCharacterMovement");

            List<ChunkLocation> ungeneratedChunks = new List<ChunkLocation>();
            foreach (var chunkLoc in PlayerBoundaryChunks(to, PLAYER_WORLDGEN_RADIUS))
            {
                var _chunk = WorldSystem.Chunk(chunkLoc, out CbTileState cbTileState);
                if (cbTileState == CbTileState.OutOfBounds) ungeneratedChunks.Add(chunkLoc);
            }
            if (ungeneratedChunks.Count > 0)
            {
                foreach (var chunkLoc in ungeneratedChunks)
                {
                    WorldSystem.Get.GenerateChunk(chunkLoc);
                }
            }
        }

        private IEnumerable<ChunkLocation> PlayerBoundaryChunks(WorldPoint Origin, int Distance)
        {
            ChunkLocation chunkLoc = Origin;
            int xMin = chunkLoc.X - Distance;
            int xMax = chunkLoc.X + Distance;
            int yMin = chunkLoc.Y - Distance;
            int yMax = chunkLoc.Y + Distance;

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (x == xMin || x == xMax) { yield return new ChunkLocation(x, y); } // Left
                    //if(x == xMax) { yield return new WorldPoint(x, y); } // Right
                    else if (y == yMin || y == yMax) { yield return new ChunkLocation(x, y); } // Bottom
                    //if(y == yMax) { yield return new WorldPoint(x, y); } // Top
                }
            }

        }

        private IEnumerable<WorldPoint> PlayerBoundary(WorldPoint Origin, int Distance)
        {
            int xMin = Origin.X - Distance;
            int xMax = Origin.X + Distance;
            int yMin = Origin.Y - Distance;
            int yMax = Origin.Y + Distance;

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if(x == xMin || x == xMax) { yield return new WorldPoint(x, y); } // Left
                    //if(x == xMax) { yield return new WorldPoint(x, y); } // Right
                    else if(y == yMin || y == yMax) { yield return new WorldPoint(x, y); } // Bottom
                    //if(y == yMax) { yield return new WorldPoint(x, y); } // Top
                }
            }
        }


        /*public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ITileData Data = CursorSystem.CursorTile;
                if (Data != null)
                {
                    log.Trace($"Creating Move Action To {Data.Coordinates}");
                    MoveToWaypointTask newTask = new MoveToWaypointTask(Data.Coordinates);
                    if (selectedCreature != null)
                    {
                        log.Trace($"Creating Interrupt Move..");
                        
                    }
                    else
                    {                       
                        TaskSystem.Delegate(newTask);
                    }                   
                }
            }
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ITileData Data = CursorSystem.CursorTile;
                if (Data != null)
                {
                    if (Data.Creature != null)
                    {
                        log.Trace($"Selected Creature!");
                        selectedCreature = Data.Creature;
                    }
                    else
                    {
                        IEntity TopEntity = Data.Container.First;
                        if (TopEntity != null)
                        {
                            log.Trace($"Selected {TopEntity.DefName}");
                            var item = TopEntity.FindTrait<ITraitContainable>();
                            if (item != null && item is ITraitContainable containable)
                            {
                                log.Trace($"Picking up {TopEntity.DefName}");
                                MoveToPickupItem newTask = new MoveToPickupItem(Data.Coordinates, TopEntity);
                                if (selectedCreature != null) TaskSystem.Assign(newTask, selectedCreature.AI, TaskAssignmentMethod.CLEAR);
                                else TaskSystem.Delegate(newTask);
                            }
                        }
                        selectedCreature = null;
                    }
                }
            }
        }*/

        #region Character Selector

       /* private MeshData meshData;
        private Material selectionOverlayMat;

        private void RenderCharacterSelector()
        {
            if (selectedCreature != null)
            {
                Vector3 selectionPos = selectedCreature.RenderPoint;
                Graphics.DrawMesh(
                            meshData.mesh,
                            new Vector3(selectionPos.x, selectionPos.y, -(int)EntityLayer.ABOVE),
                            Quaternion.identity,
                            selectionOverlayMat,
                            0
                         );
            }
        }

        private void BuildSelectionOverlayMesh()
        {
            this.selectionOverlayMat = new Material(ResourceManager.LoadEntityMaterial("basic"));
            Texture2D Texture = ResourceManager.LoadUtilityTexture("utility.selection-overlay");
            this.selectionOverlayMat.mainTexture = Texture;
            if (meshData == null) meshData = new MeshData(1, MeshFlags.ALL);
            else { meshData.Clear(); }

            int vIndex = meshData.vertices.Count;

            meshData.vertices.Add(new Vector3(0, 0));
            meshData.vertices.Add(new Vector3(0, 1));
            meshData.vertices.Add(new Vector3(1, 1));
            meshData.vertices.Add(new Vector3(1, 0));

            meshData.UVs.Add(new Vector3(0, 0));
            meshData.UVs.Add(new Vector2(0, 1));
            meshData.UVs.Add(new Vector2(1, 1));
            meshData.UVs.Add(new Vector2(1, 0));

            meshData.AddTriangle(vIndex, 0, 1, 2);
            meshData.AddTriangle(vIndex, 0, 2, 3);

            meshData.colors.Add(new Color(1, 1, 1, 0.75f));
            meshData.colors.Add(new Color(1, 1, 1, 0.75f));
            meshData.colors.Add(new Color(1, 1, 1, 0.75f));
            meshData.colors.Add(new Color(1, 1, 1, 0.75f));

            meshData.Build();
        }*/

        #endregion

        #region Gizmos

        /*[SerializeField]
        private bool drawPlayerWorldGenRadius;

        public void OnDrawGizmos()
        {
            if (Initialized)
            {
                if (drawPlayerWorldGenRadius)
                {
                    if (PlayerCharacters != null && PlayerCharacters.Count > 0)
                    {
                        foreach (var player in PlayerCharacters)
                        {
                            foreach (var chunkLoc in PlayerBoundaryChunks(player.Navigation.Coordinates, PLAYER_WORLDGEN_RADIUS))
                            {
                                RectI rect = new RectI(new Vector2Int(chunkLoc.X, chunkLoc.Y) * new Vector2Int(WorldSystem.CHUNK_SIZE, WorldSystem.CHUNK_SIZE), WorldSystem.CHUNK_SIZE, WorldSystem.CHUNK_SIZE);
                                Gizmos.color = new Color(1, 0, 1, 0.5f);
                                Gizmos.DrawCube(
                            new Vector3(rect.max.x - (rect.width) / 2f,
                                        rect.max.y - (rect.height) / 2f,
                                        1f),
                            new Vector3(rect.width - .5f, rect.height - .5f, 1f)
                            );
                            }
                        }
                    }
                }
            }
        }*/
    }
    #endregion
}
