using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GalaxyGame.GameStates
{
    class MenuState : GameState
    {
        private LonelyButton _startButton;

        public MenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content) : base(game, graphicsDevice, content)
        {
            _startButton = new LonelyButton(_content.Load<Texture2D>("Background/LonelyButton"), _content.Load<SpriteFont>("Fonts/GameInfo"));
            int button_x = graphicsDevice.Viewport.Width / 2 - _startButton.Rectangle.Width / 2;
            int button_y = graphicsDevice.Viewport.Height / 2 - _startButton.Rectangle.Height / 2;
            _startButton.Position = new Vector2(button_x, button_y);
            _startButton.Click += StartGameClick;
        }

        private void StartGameClick(object sender, EventArgs e)
        {
            //TODO START GAME!!!
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            _startButton.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            _startButton.Update(gameTime);
        }
        public override void PostUpdate(GameTime gameTime)
        {
            
        }

  
    }
}
