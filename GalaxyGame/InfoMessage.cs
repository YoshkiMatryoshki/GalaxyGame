using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
     class InfoMessage : Sprite
    {
        private LonelyButton _button;
        
        
        
        public SpriteFont _headingFont;
        private Vector2 _headerPosition;
        
        public bool Accepted;
        public string Text;

        public InfoMessage(Texture2D texture, LonelyButton bttn, Vector2 position) : base(texture)
        {
            Accepted = false;
            _button = bttn;
            Position = position;
            _headerPosition = new Vector2(Position.X + 30, Position.Y + 10);
            float button_x = Position.X + _texture.Width - (_button.Rectangle.Width + 6);
            float button_y = Position.Y + _texture.Height - (_button.Rectangle.Height + 5);
            _button.Position = new Vector2(button_x, button_y);
            _button.Click += GameOverAccepted;
            _button.Text = "Ok";
        }

        private void GameOverAccepted(object sender, EventArgs e)
        {
            Accepted = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            _button.Draw(null,spriteBatch);
            spriteBatch.DrawString(_headingFont, Text,_headerPosition , Color.Black);
        }

        public void Update()
        {
            _button.Update(null);
        }

    }
}
