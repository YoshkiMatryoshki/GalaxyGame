using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    public class BombRectangle : Sprite
    {
        private float _timer;
        private float _bombDelay;

        public BombRectangle(Texture2D texture,float bombDelay) : base(texture)
        {
            _timer = 0;
            _bombDelay = bombDelay;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprite)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer > _bombDelay)
            {
                IsRemoved = true;
                foreach(Sprite sp in sprite)
                {
                    if (rectangle.Contains(sp.rectangle))
                    {
                        sp.IsRemoved = true;
                    }
                }
            }


        }


    }
}
