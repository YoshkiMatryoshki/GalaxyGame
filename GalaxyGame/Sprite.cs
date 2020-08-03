using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    //Main sprite class
     public class Sprite : ICloneable
    {
        protected Texture2D _texture;
        public MainGameState ParentState; 
        public Vector2 Position;
        public Vector2 Origin;
        public float speed;


        public bool IsRemoved = false;
        public Rectangle rectangle
        {
            get
            {
                //Возвращает "хитбокс" спруйта для чека коллизии
                return new Rectangle(Position.ToPoint(), new Point(_texture.Width, _texture.Height));
            }
        }
  


        public Sprite(Texture2D texture)
        {
            _texture = texture;
        }

        public virtual void Update(GameTime gameTime, List<Sprite> sprite)
        {

        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Color.White);
        }
        public virtual void MatchDetection(GameTime gameTime, List<Sprite> sprite)
        {

        }
        public virtual void FishForClick()
        {

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
