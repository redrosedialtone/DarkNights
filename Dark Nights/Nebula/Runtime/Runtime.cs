using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Nebula.Program.Tiles;
using System.Runtime.InteropServices;
using Nebula.Systems;

namespace Nebula.Main
{
    public class Runtime : Game
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("RUNTIME");
        public static GraphicsDeviceManager Graphics;
        public static ContentManager GameContent => Access.Content;
        public static Runtime Access;
        public static string dataPath;

        private IControl[] Controls;

        public Runtime()
        {
            Access = this;
            log.Info("Runtime Executed...");

            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Controls = new IControl[5];
            Controls[0] = new Graphics();
            Controls[1] = new Interface();
            Controls[2] = new Input();
            Controls[3] = new Time();
            Controls[4] = new ApplicationController();
            Controls[0].Create(this);
            Controls[1].Create(this);
            Controls[2].Create(this);
            Controls[3].Create(this);
            Controls[4].Create(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            log.Info("Initializing Runtime Controls...");
            foreach (var control in Controls)
            {
                control.Initialise();
            }            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            foreach (var control in Controls)
            {
                control.LoadContent();
            }
        }

        protected override void UnloadContent()
        {
            foreach (var control in Controls)
            {
                control.UnloadContent();
            }
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                foreach (var control in Controls)
                {
                    control.Update(gameTime);
                }
                base.Update(gameTime);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                log.Info("Goodbye Cruel World!");
                ExitApplication();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {
                foreach (var control in Controls)
                {
                    control.Draw(gameTime);
                }
                base.Draw(gameTime);
            }
        }

        public void ExitApplication()
        {
            log.Info("> APPLICATION CLOSED <");
            NLog.LogManager.Shutdown();
            Exit();
        }
    }
}
