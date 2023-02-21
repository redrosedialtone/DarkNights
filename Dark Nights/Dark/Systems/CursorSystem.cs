using Microsoft.Xna.Framework;
using Nebula;
using Nebula.Main;
using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class CursorSystem : Manager
    {
        #region Static
        private static CursorSystem instance;
        public static CursorSystem Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("CURSOR");
        #endregion

        //private MeshData meshData;
        //private Material selectionOverlayMat;

        public override void Init()
        {
            log.Info("> Cursor System Init <");
            instance = this;
            base.Init();
        }

        public WorldPoint currentMousePosition;
        public WorldPoint oldMousePosition;
        public static ITileData CursorTile => instance.highlightedTile;

        public ITileData highlightedTile;

        public override void Tick()
        {
            UpdateMousePositions();
            if (highlightedTile == null)
            {
                return;
            }
            //RenderSelectionOverlay();
            base.Tick();
        }

        private void UpdateMousePositions()
        {
            oldMousePosition = currentMousePosition;
            Vector2 mousePos = Input.Access.MousePointerEventData.Position.ToVector2();
            WorldPoint worldPoint = WorldSystem.ToWorldPoint(mousePos);
            if (worldPoint != currentMousePosition)
            {
                currentMousePosition = worldPoint;
                highlightedTile = WorldSystem.Tile(worldPoint, out CbTileState cbTileState);
                if (cbTileState == CbTileState.OutOfBounds) return;
                log.Trace($"MouseVector {mousePos} translates to {worldPoint}");
            }         
        }

        /*private void RenderSelectionOverlay()
        {
            if (highlightedTile != null)
            {
                Vector3 selectionPos = currentMousePosition;
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
            if(meshData == null) meshData = new MeshData(1, MeshFlags.ALL);
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
    }
}
