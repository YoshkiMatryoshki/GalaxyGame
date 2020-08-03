using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame.GameStates
{
    public abstract class GameState
    {
        protected ContentManager _content;
        protected GraphicsDevice _graphicsDevice;
        protected Game1 _mainGame;


        public GameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            _mainGame = game;
            _graphicsDevice = graphicsDevice;
            _content = content;
        }


        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public abstract void PostUpdate(GameTime gameTime);

        public abstract void Update(GameTime gameTime);

    }
}
