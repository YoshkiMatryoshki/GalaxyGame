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

        //static factory method
        public static LineBonus CreateBonusByPlanet(Planet basePlanet, Vector2 lineBonusDest)
        {
            Texture2D texture;
            if (lineBonusDest.X == 1)
                texture = MainGameState.LinePlanetTextures[(int)basePlanet.planetType];
            else
                texture = MainGameState.LinePlanetTextures[(int)basePlanet.planetType + 5];
            LineBonus newBonus = new LineBonus(texture)
            {
                planetType = basePlanet.planetType,
                Position = basePlanet.Position,
                BonusDirection = lineBonusDest
            };
            return newBonus;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            base.Update(gameTime, sprites);
            if (IsRemoved)
            {
                MainGameState.FreezeField = true;
                
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
