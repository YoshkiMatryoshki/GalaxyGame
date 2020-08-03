using GalaxyGame.GameStates;
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
        private float _fallingSpeed = 6f;
        //private float _fallingDistance = 0; //дистанция падения элемента - будет влиять на отскок


        public Vector2 Destinaition;
        //private bool _bounce = false;

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
            if (MainGameState.FieldHasNoMatches || MainGameState.IsDestroyerActive)
                return;


            //Падение элемента!
            float bot = MainGameState.gameGrid.BottomLine;
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
            Position.Y = MathHelper.Clamp(Position.Y, -1000, bot - MainGameState.gameGrid.BorderSize - _texture.Height);
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
            List<Sprite> this_columnsprites = sprites.Where(sprite => sprite.rectangle.Left == this.rectangle.Left && sprite.rectangle.Bottom < MainGameState.gameGrid.Location.Y)
                .Select(x => x).ToList();
            if (this_columnsprites.Count == 0)
                return;
            int res = this_columnsprites.Min(x => x.rectangle.Top);
            Position.Y = res - MainGameState.gameGrid.BorderSize - _texture.Height;
        }
        private void Rotate()
        {
            Position.X = (float)(Origin.X + Math.Cos(_angle) * radius);
            Position.Y = (float)(Origin.Y + Math.Sin(_angle) * radius);
            _angle -= _angleSpeed;
        }


        public override void FishForClick()
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState().LeftButton;
            //Установка кликнутых элементов
            if (_currentState == ButtonState.Pressed && _previousState == ButtonState.Released)
            {
                bool IsItME = rectangle.Contains(Mouse.GetState().Position);
                if (IsItME)
                {
                    if (MainGameState.CurrentClickedPlanet == null)
                    {
                        IsClicked = true;
                        MainGameState.CurrentClickedPlanet = this;
                        //Game1.IsElementClicked = true;
                    }
                    else if (MainGameState.CurrentClickedPlanet == this)
                    {
                        //Game1.IsElementClicked = false;
                        MainGameState.CurrentClickedPlanet = null;
                        MoveToOriginPlace();
                    }
                    if (MainGameState.CurrentClickedPlanet != null && MainGameState.CurrentClickedPlanet != this)
                    {
                        MainGameState.SecondPlanet = this;
                    }

                }
            }
            //Вращение выделенного элемента
            if (IsClicked == true)
            {
                Rotate();
            }
        }

        #region MatchDetection
        public override void MatchDetection(GameTime gameTime, List<Sprite> sprites)
        {
            if (IsRemoved == false && GetType() != typeof(Destroyer) && GetType() != typeof(Bomb))
            {
                int indexOf;
                List<Sprite> cleared_sprites = sprites.Where(sp => sp.GetType() != typeof(Bomb) && sp.GetType() != typeof(Destroyer)).Select(x => x).ToList();

                //Горизонтальная проверка
                List<Sprite> x_neighbours = cleared_sprites.Where(sprite => sprite.rectangle.Top == rectangle.Top)
                    .OrderBy(x => x.Position.X).ToList();
                indexOf = x_neighbours.IndexOf(this);
                int right_matches = GetMatchElements(x_neighbours, indexOf, 1);
                int left_matches = GetMatchElements(x_neighbours, indexOf, -1);
                SpawnBombsDeleteMatches(sprites, ref indexOf, x_neighbours, ref right_matches, left_matches, new Vector2(1,0));

                ////Вертикальная проверка
                List<Sprite> y_neighbours = cleared_sprites.Where(sprite => sprite.Position.X == Position.X)
                    .OrderBy(x => x.Position.Y).ToList();
                indexOf = y_neighbours.IndexOf(this);
                int bot_matches = GetBottomMatchElements(y_neighbours, indexOf, 1);
                int top_matches = GetBottomMatchElements(y_neighbours, indexOf, -1);
                SpawnBombsDeleteMatches(sprites, ref indexOf, y_neighbours, ref bot_matches, top_matches, new Vector2(0, 1));
            }
        }
        // Right + left = размер матча
        // в зависимости от размера спавнит новые элементы
        //и удаляет матчи IsRemoved = true;
        private void SpawnBombsDeleteMatches(List<Sprite> sprites, ref int indexOf, List<Sprite> x_neighbours, ref int right_matches, int left_matches, Vector2 destination)
        {
            if (right_matches + left_matches > 3)
            {
                Bomb bomb = new Bomb(MainGameState.BombTexture)
                {
                    Position = this.Position,
                    planetType = PlanetType.BlackHole
                };
                MainGameState.spriteSpawner.AddBonus(bomb, sprites);
            }
            else if (right_matches + left_matches == 3)
            {
                Texture2D texture;
                if (destination.X == 1)
                    texture = MainGameState.LinePlanetTextures[(int)this.planetType];
                else
                    texture = MainGameState.LinePlanetTextures[(int)this.planetType + 5];
                LineBonus temp_sprite = new LineBonus(texture)
                {
                    planetType = this.planetType,
                    Position = this.Position,
                    BonusDirection = destination
                };
                MainGameState.spriteSpawner.AddBonus(temp_sprite, sprites);
            }
            if (right_matches + left_matches >= 2)
            {
                right_matches += indexOf;
                indexOf -= left_matches;
                while (indexOf <= right_matches)
                {
                    x_neighbours[indexOf].IsRemoved = true;
                    indexOf++;
                }
            }
        }

        //Собирает по списку комбинацию из совпадающих элементов по горизонтали

        private int GetMatchElements(List<Sprite> sprites, int start_ind, int destination)
        {
            int count = 0;
            Planet curr_elem;
            Planet next_elem;
            try
            {
                curr_elem = sprites[start_ind] as Planet;
                next_elem = sprites[start_ind + destination] as Planet;
            }
            catch
            {
                return 0;
            }
            if (curr_elem.planetType != next_elem.planetType)
            {
                return 0;
            }
            else if (destination == 1 && curr_elem.IsRightElement(next_elem))
            {
                return 1 + GetMatchElements(sprites, start_ind + destination, destination);
            }
            else if (destination == -1 && curr_elem.IsLeftElement(next_elem))
            {
                return 1 + GetMatchElements(sprites, start_ind + destination, destination);
            }
            return count;
        }
        private int GetBottomMatchElements(List<Sprite> sprites, int start_ind, int destination)
        {
            int count = 0;
            Planet curr_elem;
            Planet next_elem;
            try
            {
                curr_elem = sprites[start_ind] as Planet;
                next_elem = sprites[start_ind + destination] as Planet;
            }
            catch
            {
                return 0;
            }
            if (curr_elem.planetType != next_elem.planetType)
            {
                return 0;
            }
            else if (destination == 1 && curr_elem.IsBottomElement(next_elem))
            {
                return 1 + GetBottomMatchElements(sprites, start_ind + destination, destination);
            }
            else if (destination == -1 && curr_elem.IsTopElement(next_elem))
            {
                return 1 + GetBottomMatchElements(sprites, start_ind + destination, destination);
            }
            return count;
        }

        #endregion


        #region Коллизии
        //Проверка на коллизию с правым элементов
        private bool IsRightElement(Sprite sprite)
        {
            return (sprite.rectangle.Top <= rectangle.Y + rectangle.Height + 15 && sprite.rectangle.Top >= rectangle.Y - rectangle.Height - 15
                && (Math.Abs(sprite.rectangle.Left - rectangle.Right) <= (MainGameState.gameGrid.BorderSize + _texture.Width / 8)));
        }
        private bool IsLeftElement(Sprite sprite)
        {
            return (sprite.rectangle.Top <= rectangle.Y + rectangle.Height + 15 && sprite.rectangle.Top >= rectangle.Y - rectangle.Height - 15
                && (Math.Abs(sprite.rectangle.Right - rectangle.Left) <= (MainGameState.gameGrid.BorderSize + _texture.Width / 8)));
        }
        //Проверка на коллизию с нижним элементом
        //В пределах колонны 
        private bool IsBottomElement(Sprite sprite)
        {
            return (sprite.rectangle.Left >= rectangle.Left && sprite.rectangle.Left <= rectangle.Right
                || sprite.rectangle.Right <= rectangle.Right && sprite.rectangle.Right >= rectangle.Left)
                &&
                (Math.Abs(sprite.rectangle.Top - rectangle.Bottom) <= (MainGameState.gameGrid.BorderSize + _texture.Height / 2));
        }
        private bool IsTopElement(Sprite sprite)
        {
            return (sprite.rectangle.Left >= rectangle.Left && sprite.rectangle.Left <= rectangle.Right
                || sprite.rectangle.Right <= rectangle.Right && sprite.rectangle.Right >= rectangle.Left)
                &&
                (Math.Abs(sprite.rectangle.Bottom - rectangle.Top) <= (MainGameState.gameGrid.BorderSize + _texture.Height / 2));
        }

        #endregion
    }
}
