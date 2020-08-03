using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame.GameStates
{
    class LonelyButton
    {
        private MouseState _currMouseState;
        private MouseState _prevMouseState;
        private SpriteFont _font;
        private Texture2D _texture;


        public event EventHandler Click;
        public Vector2 Position { get; set; }

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
            }
        }
        public string Text { get; set; }

        public LonelyButton(Texture2D texture,SpriteFont font)
        {
            _texture = texture;
            _font = font;
        }
        public void Draw(GameTime gameTime,SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Rectangle, Color.White);

            if(Text != null || Text != "")
            {
                var x = (Rectangle.X + (Rectangle.Width / 2)) - (_font.MeasureString(Text).X / 2);
                var y = (Rectangle.Y + (Rectangle.Height / 2)) - (_font.MeasureString(Text).Y / 2);

                spriteBatch.DrawString(_font, Text, new Vector2(x, y), Color.Black);
            }
        }

        public void Update(GameTime gameTime)
        {
            _prevMouseState = _currMouseState;
            _currMouseState = Mouse.GetState();
            
            
            if (Rectangle.Contains(_currMouseState.Position))
            {
                if (_currMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed)
                {
                    Click?.Invoke(this, new EventArgs());
                }
            }

        }
    }
}
