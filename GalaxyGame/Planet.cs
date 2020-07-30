using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    public enum PlanetType
    {
        Earth = 0,
        Venus = 1,
        Mars = 2,
        Saturn = 3,
        Satelite = 4
    }
    public class Planet
    {
        private Texture2D _texture;
        private float _angle = 0; //угол относительно X в радианах
        private float _angleSpeed = 0.3f;//0.15f;
        private float _fallingSpeed = 3f;
        private float _fallingDistance = 0; //дистанция падения элемента - будет влиять на отскок

        public bool IsFalling = false;
        private bool _bounce = false;

        public Vector2 Position;
        public PlanetType planetType;
        public bool IsClicked = false;
        public Vector2 Origin;

        public Rectangle Rectangle
        {
            get
            {
                //Возвращает "хитбокс" планеты для чека коллизии
                return new Rectangle(Position.ToPoint(), new Point(_texture.Width, _texture.Height));
            }
        }

        public Planet(Texture2D texture, Vector2 position)
        {
            _texture = texture;
            Position = position;
            Origin = new Vector2(Position.X - 10, Position.Y +  _texture.Height/2);     
            
        }

        //По аналогии с основным классом Update -1st/ Draw -2nd
        public void Update()
        {
            if (IsClicked)
            {
                Position.X = (float)(Origin.X + Math.Cos(_angle) * 5);
                Position.Y = (float)(Origin.Y + Math.Sin(_angle) * 5);
                _angle += _angleSpeed;
            }
            //Падение элемента
            if (IsFalling)
            {
                Bounce();
            }
            

        }    
        //Падение и отскок элемента
        private void Bounce()
        {
            if (Position.Y <= 400 && _bounce == false)
            {
                Position.Y += _fallingSpeed;
                _fallingSpeed += 0.05f;
                _fallingDistance += 0.03f;
            }
            else
            {
                _bounce = true;
            }
            //Отчаянная попытка сделать отскок
            if (_bounce == true && _fallingDistance >= 0)
            {
                Position.Y -= _fallingSpeed;
                _fallingSpeed -= 0.13f;
                _fallingDistance -= 0.2f;
            }
            else
            {
                _bounce = false;
            }

            //Тест ускорения свободного падения!!
            //if (Position.Y < 400)
            //{
            //    Position.Y += _fallingSpeed;
            //}
            //else
            //{
            //    Position.Y = 0;
            //}
            //_fallingSpeed += 0.05f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Color.White);
            //spriteBatch.Draw(_texture, Position, null, Color.White, _angleSpeed, Origin,1, SpriteEffects.None, 0f);
        }
    }
}
