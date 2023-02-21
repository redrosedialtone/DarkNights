using Microsoft.Xna.Framework;
using Nebula.Input;
using Nebula.Main;
using System;
using Nebula.Content;
using Nebula.Base;
using Microsoft.Xna.Framework.Graphics;

namespace Nebula.Program.Tiles
{
    public class TileController : IControl, IPointerClickHandler, IDrawUIBatch
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        Texture2D tileTexture;
        Runtime RUNTIME;
        Coordinate lastPosition;
        bool _draw = false;

        public TileController()
        {

        }

        public IPointerEventListener Parent => null;
        public IPointerEventListener[] Children => null;

        public void Create(Runtime game)
        {
            Nebula.Main.Input.AddPointerEventListener(this);
            RUNTIME = game;
        }

        public void Draw(GameTime gameTime)
        {
            
        }

        public void DrawUI(SpriteBatch Batch)
        {
            if (_draw)
            {
                Batch.Draw(tileTexture, new Vector2(lastPosition.X, lastPosition.Y), Color.White);
            }           
        }

        public void Initialise()
        {
            Graphics.AddBatchDraw(this);
        }

        public IPointerEventListener Intersect(Point mousePos) => this;

        public void LoadContent()
        {
            tileTexture = RUNTIME.Content.Load<Texture2D>("IMG/basicStructureFull");
        }

        public bool PointerClick(MouseButtonEventData Data)
        {
            _draw = true;
            lastPosition = new Coordinate(Data.mousePosition);
            log.Trace("Pointer Click::"+ lastPosition);
            return true;
        }

        public void UnloadContent()
        {
           
        }

        public void Update(GameTime gameTime)
        {
            
        }
    }
}
