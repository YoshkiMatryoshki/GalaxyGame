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
        
        private SpriteFont _headingFont;
        private Vector2 _fontPosition;


        private static string _header = "GALAXY GAME";
        public MenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content) : base(game, graphicsDevice, content)
        {
            _startButton = new LonelyButton(_content.Load<Texture2D>("Background/LonelyButton"), _content.Load<SpriteFont>("Fonts/ButtonFont"));
            int button_x = graphicsDevice.Viewport.Width / 2 - _startButton.Rectangle.Width / 2;
            int button_y = graphicsDevice.Viewport.Height / 2 - _startButton.Rectangle.Height / 2;
            _startButton.Position = new Vector2(button_x, button_y);
            _startButton.Click += StartGameClick;
            _startButton.Text = "Start";

            _headingFont = _content.Load<SpriteFont>("Fonts/BigBoyFont");
            float font_x = graphicsDevice.Viewport.Width / 2 - (_headingFont.MeasureString(_header).X / 2);
            float font_y = graphicsDevice.Viewport.Bounds.Top + 170;
            _fontPosition = new Vector2(font_x, font_y);
        }

        private void StartGameClick(object sender, EventArgs e)
        {
            _mainGame._nextGameState = _mainGame.MainGame;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            if (BackGroundTexture != null)
            {
                spriteBatch.Draw(BackGroundTexture, new Vector2(0, 0), Color.White);
            }
            _startButton.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(_headingFont,_header,_fontPosition, Color.White);
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
