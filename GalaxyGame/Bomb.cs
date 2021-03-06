﻿using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    class Bomb : Planet
    {
        public static BombRectangle RememberedRect;
        private static int _gridBorder = MainGameState.gameGrid.BorderSize;
        
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
            if (IsRemoved == true)
            {
                
                RememberedRect.Position = new Vector2(_destroyRect.Location.X,_destroyRect.Location.Y);

                MainGameState.bombRectangles.Add(RememberedRect.Clone() as BombRectangle);
            }

            base.Update(gameTime, sprites);
        }



    }
}
