using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    //Лайн бонус, являющийся "усиленной" версией обычной планеты, взрывающий всю линию
    class LineBonus : Planet
    {

        public LineBonus(Texture2D texture) : base(texture)
        {
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            base.Update(gameTime, sprites);
        }
        //public override void Draw(SpriteBatch spriteBatch)
        //{
        //    base.Draw(spriteBatch);
        //}
    }
}
