using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    //Лайн бонус, являющийся "усиленной" версией обычной планеты, взрывающий всю линию
    class LineBonus : Planet
    {
        public Vector2 BonusDirection;
        public static Destroyer Destroyer;
        public LineBonus(Texture2D texture) : base(texture)
        {
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            base.Update(gameTime, sprites);
            if (IsRemoved == true)
            {
                MainGameState.IsDestroyerActive = true;
                
                //1st one
                Destroyer destroyer = Destroyer.Clone() as Destroyer;
                //destroyer.speed = 3f;
                destroyer.Parent = this;
                destroyer.Destination = BonusDirection;
                destroyer.Position = this.Position;
                sprites.Add(destroyer);

                //2nd one
                Destroyer destroyer1 = Destroyer.Clone() as Destroyer;
                //destroyer1.speed = 3f;
                destroyer1.Parent = this;
                destroyer1.Destination = -BonusDirection;
                destroyer1.Position = this.Position;
                sprites.Add(destroyer1);
            }

        }
        //public override void Draw(SpriteBatch spriteBatch)
        //{
        //    base.Draw(spriteBatch);
        //}
    }
}
