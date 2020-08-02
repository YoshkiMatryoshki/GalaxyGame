using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace GalaxyGame
{

    public class Planet : Sprite
    {
        public static float radius;
        private float _angle = MathHelper.ToRadians(141); //угол относительно X в радианах
        private float _angleSpeed = 0.15f;//0.15f;
        private float _fallingSpeed = 5f;
        private float _fallingDistance = 0; //дистанция падения элемента - будет влиять на отскок


        public Vector2 Destinaition;
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

            //Swap элементов
            if (Destinaition.X != 0 && Destinaition.Y != 0 && Position != Destinaition)
            {
                Vector2 DirectionVector = Destinaition - Position;
                float speedVector;

                if (DirectionVector.Length() < _fallingSpeed)
                    speedVector = DirectionVector.Length();
                else
                    speedVector = _fallingSpeed;

                DirectionVector.Normalize();
                Position += DirectionVector * speedVector;
                return;
            }
            else
            {
                Destinaition.X = 0;
                Destinaition.Y = 0;
            }

            //Установка кликнутых элементов
            if (_currentState == ButtonState.Pressed && _previousState == ButtonState.Released)
            {
                bool IsItME = rectangle.Contains(Mouse.GetState().Position);
                if (IsItME)
                {
                    if (Game1.CurrentClickedPlanet == null)
                    {
                        IsClicked = true;
                        Game1.CurrentClickedPlanet = this;
                        Game1.IsElementClicked = true;
                    }
                    else if (Game1.CurrentClickedPlanet == this)
                    {
                        Game1.IsElementClicked = false;
                        Game1.CurrentClickedPlanet = null;
                        MoveToOriginPlace();
                    }
                    if (Game1.CurrentClickedPlanet!= null && Game1.CurrentClickedPlanet != this)
                    {
                        Game1.SecondPlanet = this;
                    }

                }
            }
            
            //Вращение выделенного элемента
            if (IsClicked == true)
            {
                Rotate();
                return;
            }

            //Нет гравитации, если на форме выделен элемент
            if (Game1.IsElementClicked == true)
                return;

            //Падение элемента!
            float bot = Game1.gameGrid.BottomLine;
            foreach (Sprite sprite in sprites)
            {
                if (sprite == this || sprite.GetType() == typeof(Destroyer))
                    continue;
                if (IsBottomElement(sprite))
                {
                    bot = sprite.rectangle.Top;
                    break;
                }
            }
            Position.Y += _fallingSpeed;
            Position.Y = MathHelper.Clamp(Position.Y, -1000, bot - Game1.gameGrid.BorderSize - _texture.Height);
            Origin = new Vector2(Position.X + _texture.Width / 8, Position.Y + _texture.Height / 8);


        }

        //Отключение вращения и возврат в стартовую точку.
        public void MoveToOriginPlace()
        {
            IsClicked = false;
            Position.X = Origin.X - _texture.Width / 8;
            Position.Y = Origin.Y - _texture.Height / 8;
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

        #region MatchDetection
        public override void MatchDetection(GameTime gameTime, List<Sprite> sprites)
        {
            if (IsRemoved == false && GetType() != typeof(Destroyer) || GetType() != typeof(Bomb))
            {
                List<Sprite> cleared_sprites = sprites.Where(sp => sp.GetType() != typeof(Bomb) && sp.GetType() != typeof(Destroyer)).Select(x => x).ToList();

                //Горизонтальная проверка
                List<Sprite> right_neighbours = cleared_sprites.Where(sprite => sprite.rectangle.Top == rectangle.Top && sprite.Position.X >= Position.X)
                    .OrderBy(x => x.Position.X).ToList();
                int xd = GetMatchNumbers(right_neighbours, 0);
                if (xd == 3)
                {
                    sprites.Add(new LineBonus(Game1.LinePlanetTextures[(int)this.planetType])
                    {
                        planetType = this.planetType,
                        Position = this.Position,
                        BonusDirection = new Vector2(1, 0)
                    });
                }
                if (xd >= 2)
                {
                    IsRemoved = true;
                    while (xd > 0)
                    {
                        right_neighbours[xd].IsRemoved = true;
                        xd--;
                    }
                }
                xd = 0;
                //Вертикальная проверка
                List<Sprite> bot_neighbours = cleared_sprites.Where(sprite => sprite.Position.X == Position.X && sprite.Position.Y >= Position.Y)
                    .OrderBy(x => x.Position.Y).ToList();
                xd = GetBottomMatchNumbers(bot_neighbours, 0);
                if (xd == 3)
                {
                    sprites.Add(new LineBonus(Game1.LinePlanetTextures[(int)this.planetType])
                    {
                        planetType = this.planetType,
                        Position = this.Position,
                        BonusDirection = new Vector2(0, 1)
                    });
                }
                if (xd >= 2)
                {
                    IsRemoved = true;
                    while (xd > 0)
                    {
                        bot_neighbours[xd].IsRemoved = true;
                        xd--;
                    }
                }
            }
        }
        #endregion



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


        //Собирает по списку комбинацию из совпадающих элементов по горизонтали
        private int GetMatchNumbers(List<Sprite> sprites, int ind)
        {
            int count = 0;
            Planet curr_elem = sprites[ind] as Planet;
            Planet next_elem;
            try
            {
                next_elem = sprites[ind + 1] as Planet;
            }
            catch
            {
                return 0;
            }
            if (curr_elem.planetType != next_elem.planetType)
            {
                return 0;
            }
            else if (curr_elem.IsRightElement(next_elem))
            {
                return 1 + GetMatchNumbers(sprites, ind + 1);
            }
            return count;
        }
        //Собирает по списку комбинацию из совпадающих элементов
        private int GetBottomMatchNumbers(List<Sprite> sprites, int ind)
        {
            int count = 0;
            Planet curr_elem = sprites[ind] as Planet;
            Planet next_elem;
            try
            {
                next_elem = sprites[ind + 1] as Planet;
            }
            catch
            {
                return 0;
            }
            if (curr_elem.planetType != next_elem.planetType)
            {
                return 0;
            }
            else if (curr_elem.IsBottomElement(next_elem))
            {
                return 1 + GetBottomMatchNumbers(sprites, ind + 1);
            }
            return count;

        }



        //Проверка на коллизию с правым элементов
        private bool IsRightElement(Sprite sprite)
        {
            return (sprite.rectangle.Top <= rectangle.Y + rectangle.Height + 15 && sprite.rectangle.Top >= rectangle.Y - rectangle.Height - 15
                && (Math.Abs(sprite.rectangle.Left - rectangle.Right) <= (Game1.gameGrid.BorderSize + _texture.Width / 8)));
        }
        //Проверка на коллизию с нижним элементом
        //В пределах колонны 
        private bool IsBottomElement(Sprite sprite)
        {
            return (sprite.rectangle.Left >= rectangle.Left && sprite.rectangle.Left <= rectangle.Right
                || sprite.rectangle.Right <= rectangle.Right && sprite.rectangle.Right >= rectangle.Left)
                &&
                (Math.Abs(sprite.rectangle.Top - rectangle.Bottom) <= (Game1.gameGrid.BorderSize + _texture.Height / 2));
        }
    }
}
