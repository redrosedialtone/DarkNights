using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nebula.Base;

namespace Nebula.Main
{
    public class Graphics : IControl
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public static Graphics Access;

        public GraphicsDeviceManager GraphicsDeviceMngr => Runtime.Graphics;
        public GraphicsDevice GraphicsDevice => RUNTIME.GraphicsDevice;
        public static int SCREEN_WIDTH = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
        public static int SCREEN_HEIGHT = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        public static int SCREEN_ASPECT => SCREEN_WIDTH / SCREEN_HEIGHT;
        public static int RENDER_WIDTH => 1366;
        public static int RENDER_HEIGHT => 768;
        public static int RENDER_ASPECT => RENDER_WIDTH / RENDER_HEIGHT;

        private List<IDrawGizmos> gizmos = new List<IDrawGizmos>();
        private List<IDrawUIBatch> UIDrawCalls = new List<IDrawUIBatch>();

        private Runtime RUNTIME;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;
        //Texture2D ballTexture;


        public void Create(Runtime game)
        {
            RUNTIME = game;
            Access = this;
        }

        public void Initialise()
        {
            //Graphics.IsFullScreen = true;
            GraphicsDeviceMngr.PreferredBackBufferWidth = RENDER_WIDTH;
            GraphicsDeviceMngr.PreferredBackBufferHeight = RENDER_HEIGHT;
            GraphicsDeviceMngr.ApplyChanges();
            renderTarget = new RenderTarget2D(GraphicsDevice, RENDER_WIDTH, RENDER_HEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
        }

        public void LoadContent()
        {
            _spriteBatch = new SpriteBatch(RUNTIME.GraphicsDevice);
            //ballTexture = RUNTIME.Content.Load<Texture2D>("DesignButtonLogo");
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target, new Color(23,23,23), 1.0f, 0);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            DrawGizmos();
            DrawSpriteBatch(_spriteBatch);
            //
            //_spriteBatch.Draw(ballTexture, new Vector2(0, 0), Color.White);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin();
            _spriteBatch.Draw(renderTarget, new Rectangle(0,0,RENDER_WIDTH,RENDER_HEIGHT), Color.White);
            _spriteBatch.End();
        }

        public static void AddGizmo(IDrawGizmos Gizmo)
        {
            Access.gizmos.Add(Gizmo);
        }

        private void DrawGizmos()
        {
            foreach (var gizmo in gizmos)
            {
                gizmo.DrawGizmos(_spriteBatch);
            }
        }

        public static void AddBatchDraw(IDrawUIBatch Batch)
        {
            Access.UIDrawCalls.Add(Batch);
        }

        private void DrawSpriteBatch(SpriteBatch Batch)
        {
            foreach (var batch in UIDrawCalls)
            {
                batch.DrawUI(Batch);
            }
        }

        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            Texture2D _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _texture.SetData(new[] { Color.White });
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(_texture, point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }


        public void UnloadContent()
        {
            return;
        }
    }
}
