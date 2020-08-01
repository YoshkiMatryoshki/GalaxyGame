using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalaxyGame
{

    public class Planet : Sprite
    {
        public static float radius;
        private float _angle = MathHelper.ToRadians(141); //угол относительно X в радианах
        private float _angleSpeed = 0.15f;//0.15f;
        private float _fallingSpeed = 4f;
        private float _fallingDistance = 0; //дистанция падения элемента - будет влиять на отскок

        public bool IsFalling = true;
        private bool _bounce = false;

        public PlanetType planetType;
        public bool IsClicked = false;

        private ButtonState _previousState;
        private ButtonState _currentState;

        public Planet(Texture2D texture) : base(texture)
        {
            radius = (float)Math.Sqrt(Math.Pow((_texture.Width / 8), 2) + Math.Pow((_texture.Height / 8), 2));
        }



        //По аналогии с основным классом Update -1st/ Draw -2nd
        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState().LeftButton;

            //if (Origin.X == 0 && Origin.Y == 0)
            //{
            //    Origin = new Vector2(Position.X + _texture.Width / 8, Position.Y + _texture.Height / 8);
            //}

            if (_currentState == ButtonState.Pressed && _previousState == ButtonState.Released)
            {
                bool res = rectangle.Contains(Mouse.GetState().Position);
                if (res && IsClicked == false && Game1.IsElementClicked == false)
                {
                    Game1.IsElementClicked = true;
                    IsClicked = true;
                }
                else if (res && IsClicked == true)
                {
                    IsClicked = false;
                    Position.X = Origin.X - _texture.Width / 8;
                    Position.Y = Origin.Y - _texture.Height / 8;
                    Game1.IsElementClicked = false;
                }
            }

            if (IsClicked == true)
            {
                Rotate();
                return;
            }


            //Нет гравитации, если на форме выделен элемент
            if (Game1.IsElementClicked == true)
                return;

            float bot = Game1.gameGrid.BottomLine;
            if (sprites.Count > 0)
            {
                bot = sprites.Min(x => x.rectangle.Top);
            }
            Position.Y += _fallingSpeed;
            Position.Y = MathHelper.Clamp(Position.Y, -1000, bot - Game1.gameGrid.BorderSize - _texture.Height);
            Origin = new Vector2(Position.X + _texture.Width / 8, Position.Y + _texture.Height / 8);


        }
        //Препятствует накладыванию спрайтов друг на друга при респавне планет
        public void SpawnCollision(List<Sprite> sprites)
        {
            List<Sprite> this_columnsprites = sprites.Where(sprite => sprite.rectangle.Left == this.rectangle.Left && sprite.rectangle.Bottom < Game1.gameGrid.Location.Y)
                .Select(x => x).ToList();
            if (this_columnsprites.Count == 0)
                return;
            int res = this_columnsprites.Min(x => x.rectangle.Top);
            Position.Y = res - Game1.gameGrid.BorderSize - _texture.Height;
        }

        private void Rotate()
        {
            Position.X = (float)(Origin.X + Math.Cos(_angle) * radius);
            Position.Y = (float)(Origin.Y + Math.Sin(_angle) * radius);
            _angle -= _angleSpeed;
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

        }
    }
}
