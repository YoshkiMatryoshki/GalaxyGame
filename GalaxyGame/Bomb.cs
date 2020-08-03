using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    class Bomb : Planet
    {
        private static int _gridBorder = MainGameState.gameGrid.BorderSize;
        private float _detonateTimer = 0;
        private Rectangle _destroyRect
        {
            get
            {
                return new Rectangle((int)(Position.X- _gridBorder - _texture.Width), (int)(Position.Y - _texture.Height - _gridBorder)
                    , _texture.Width *3, _texture.Height * 3);
            }
        }
        public Bomb(Texture2D texture) : base(texture)
        {
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            _detonateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_detonateTimer > 0.5f)
            {
                IsRemoved = true;
            }

            if (IsRemoved == true)
            {
                foreach(Sprite sp in sprites)
                {
                    if (_destroyRect.Intersects(sp.rectangle))
                    {
                        sp.IsRemoved = true;
                    }
                }
            }

            base.Update(gameTime, sprites);
        }



    }
}
