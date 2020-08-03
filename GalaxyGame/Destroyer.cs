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
        
        private  Rectangle _destroyRect
        {
            get
            {
                return new Rectangle((int)(Position.X + _texture.Width / 5), (int)(Position.Y + _texture.Height / 2), _texture.Width - 30, _texture.Height - 30);
            }
        }

        public Destroyer(Texture2D texture) : base(texture)
        {
            speed = 8f;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprite)
        {
            //Rectangle rec = new Rectangle((int)Game1.gameGrid.Location.X-100, (int)Game1.gameGrid.Location.Y, Game1.gameGrid.Width+300, Game1.gameGrid.Height+100);
            Rectangle rec = new Rectangle((int)Game1.gameGrid.Location.X, (int)Game1.gameGrid.Location.Y, Game1.gameGrid.Width, Game1.gameGrid.Height);

            if (rec.Contains(this.rectangle))
            {
                Position += Destination * speed;
            }
            else
            {
                IsRemoved = true;
            }

            //Destr does his job
            foreach(Sprite sp in sprite)
            {
                if (sp.GetType() != typeof(Destroyer))
                {
                    if (sp.rectangle.Contains(_destroyRect))
                    {
                        sp.IsRemoved = true;
                    }
                }
            }

        }
        
    }
}
