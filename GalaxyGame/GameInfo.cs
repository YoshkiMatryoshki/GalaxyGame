using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    //Выводит оставшееся время и очки игрока
    class GameInfo
    {
        private SpriteFont _spriteFont;
        private Vector2 _position1;
        private Vector2 _position2;

        public float GameTime;
        public float Score;

        public GameInfo(SpriteFont spriteFont, Vector2 position)
        {
            _spriteFont = spriteFont;
            _position1 = new Vector2(position.X + 20, position.Y + 10);
            _position2 = new Vector2(position.X+ 20, position.Y + 60);

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_spriteFont, $"TIME LEFT: {GameTime} ", _position1, Color.White);
            spriteBatch.DrawString(_spriteFont, $"SCORE: {Score} ", _position2, Color.White);
            //spriteBatch.DrawString(_spriteFont, $" {Game1.FieldHasNoMatches} ", new Vector2(_position2.X,_position2.Y+50), Color.White);
        }
    }
}
