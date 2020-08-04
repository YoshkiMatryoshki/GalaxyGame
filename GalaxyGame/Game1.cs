using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1.Effects;
using SharpDX.MediaFoundation;
using SharpDX.XInput;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyGame
{
    public enum PlanetType
    {
        Earth = 0,
        Neptune = 1,
        Mars = 2,
        Saturn = 3,
        Asteroid = 4,
        BlackHole = 5
    }
    public enum ElementToCreate
    {
        None = 0,
        LineBonus = 1,
        Bomb = 2
    }
    public class Game1 : Game
    {
        public GameState MainGame;
        public GameState Menu;
        public GameState _nextGameState;
        public GameState _currGameState;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

          
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight += 60;
            _graphics.ApplyChanges();


            base.Initialize();
        }

        protected override void LoadContent()
        {

            _spriteBatch = new SpriteBatch(GraphicsDevice);


            Texture2D backGround = Content.Load<Texture2D>("Background/fon");
            Menu = new MenuState(this, _graphics.GraphicsDevice, Content)
            {
                BackGroundTexture = backGround
            };
            MainGame = new MainGameState(this, _graphics.GraphicsDevice, Content)
            {
                BackGroundTexture = backGround
            };

            _currGameState = Menu;

        }
        public void ChangeState(GameState state)
        {
            _nextGameState = state;
        }

        protected override void Update(GameTime gameTime)
        {
            if (_nextGameState != null)
            {
                _currGameState = _nextGameState;
                _nextGameState = null;
            }

            _currGameState.Update(gameTime);
            _currGameState.PostUpdate(gameTime);


            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _currGameState.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }


    }
}
