using GalaxyGame.AnimationWorks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    class ElementExplosion : Sprite
    {
        public Animation Animation;
        public ElementExplosion(Texture2D texture) : base(texture)
        {

        }


        public override void Draw(SpriteBatch spriteBatch)
        {

        }
        public override void Update(GameTime gameTime, List<Sprite> sprite)
        {
            if (Animation.Current == Animation.FrameCount)
            {
                IsRemoved = true;
            }
            Animation.Update(gameTime);
        }
    }
}
