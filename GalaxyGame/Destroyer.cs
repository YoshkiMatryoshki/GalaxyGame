using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    //Появляется при взрыве LineBonus
    class Destroyer : Sprite
    {
        public LineBonus Parent;
        public Vector2 Destination;
        public float speed;

        public Destroyer(Texture2D texture) : base(texture)
        {
        }

        public override void Update(GameTime gameTime, List<Sprite> sprite)
        {
            
        }
        
    }
}
