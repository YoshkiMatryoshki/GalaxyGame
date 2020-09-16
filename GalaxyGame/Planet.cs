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
        private float _fallingSpeed = 7f;
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

            if (MainGameState.FieldHasNoMatches || MainGameState.FreezeField)
                return;


            //Падение элемента!
            float bot = MainGameState.gameGrid.BottomLine;
            foreach (Sprite sprite in sprites)
            {
                if (sprite == this || sprite is Destroyer)
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
            if (IsClicked)
            {
                Rotate();
            }
        }

        #region MatchDetection
        public void MatchDetection(GameTime gameTime, List<Sprite> sprites)
        {
            if (!IsRemoved)
            {
                int indexOf_X;
                //List<Sprite> cleared_sprites = sprites.Where(sp => sp.GetType() != typeof(Destroyer)).Select(x => x).ToList();
                List<Sprite> cleared_sprites = sprites.Where(sp => !(sp is Destroyer)).Select(x => x).ToList();
                Vector2 spawnPosition = this.Position;
                ElementToCreate elementToCreate = ElementToCreate.None;


                //Горизонтальная проверка
                List<Sprite> x_neighbours = cleared_sprites.Where(sprite => sprite.rectangle.Top == rectangle.Top)
                    .OrderBy(x => x.Position.X).ToList();
                indexOf_X = x_neighbours.IndexOf(this);
                int right_matches = GetMatchElements(x_neighbours, indexOf_X, 1);
                int left_matches = GetMatchElements(x_neighbours, indexOf_X, -1);
                
                int indexOf_Y;
                ////Вертикальная проверка
                List<Sprite> y_neighbours = cleared_sprites.Where(sprite => sprite.Position.X == Position.X)
                    .OrderBy(x => x.Position.Y).ToList();
                indexOf_Y = y_neighbours.IndexOf(this);
                int bot_matches = GetBottomMatchElements(y_neighbours, indexOf_Y, 1);
                int top_matches = GetBottomMatchElements(y_neighbours, indexOf_Y, -1);

                //Текущий элемент точно на пересечении
                if (right_matches+left_matches >= 2 && top_matches+bot_matches >= 2)
                {
                    spawnPosition = this.Position;
                    elementToCreate = ElementToCreate.Bomb;
                    SpawnBonus(spawnPosition, elementToCreate, new Vector2(0,0));
                    //DeleteMatchElements(right_matches, left_matches, indexOf_X, x_neighbours);
                    //DeleteMatchElements(bot_matches, top_matches, indexOf_Y, y_neighbours);
                    //return;
                }
                //Поиск потенциального пересечения двигаясь по Х
                else if (right_matches+ left_matches >= 2)
                {
                    for(int i = indexOf_X - left_matches; i <= right_matches + indexOf_X; i++)
                    {
                        int new_indexOf_Y;
                        ////Вертикальная проверка
                        List<Sprite> new_y_neighbours = cleared_sprites.Where(sprite => sprite.Position.X == x_neighbours[i].Position.X)
                            .OrderBy(x => x.Position.Y).ToList();
                        new_indexOf_Y = new_y_neighbours.IndexOf(x_neighbours[i]);
                        int new_bot_matches = GetBottomMatchElements(new_y_neighbours, new_indexOf_Y, 1);
                        int new_top_matches = GetBottomMatchElements(new_y_neighbours, new_indexOf_Y, -1);
                        if (new_bot_matches + new_top_matches < 2)
                        {
                            continue;
                        }
                        else
                        {
                            spawnPosition = x_neighbours[i].Position;
                            elementToCreate = ElementToCreate.Bomb;
                            SpawnBonus(spawnPosition, elementToCreate, new Vector2(0, 0));
                            DeleteMatchElements(new_bot_matches, new_top_matches, new_indexOf_Y, new_y_neighbours);
                            //return;
                        }
                    }
                    if (right_matches + left_matches == 3)
                    {
                        SpawnBonus(this.Position, ElementToCreate.LineBonus, new Vector2(1, 0));
                    }
                    else if (right_matches + left_matches > 3)
                    {
                        SpawnBonus(this.Position, ElementToCreate.Bomb, new Vector2(0, 0));
                    }
                    
                }
                //Поиск потенциального пересечения двигаясь по Y
                else if (top_matches + bot_matches >= 2)
                {
                    for(int i = indexOf_Y - top_matches; i <= bot_matches+ indexOf_Y; i++)
                    {
                        int new_indexOf_X;
                        //Горизонтальная проверка
                        List<Sprite> new_x_neighbours = cleared_sprites.Where(sprite => sprite.rectangle.Top == y_neighbours[i].rectangle.Top)
                            .OrderBy(x => x.Position.X).ToList();
                        new_indexOf_X = new_x_neighbours.IndexOf(y_neighbours[i]);
                        int new_right_matches = GetMatchElements(new_x_neighbours, new_indexOf_X, 1);
                        int new_left_matches = GetMatchElements(new_x_neighbours, new_indexOf_X, -1);

                        if (new_left_matches+ new_right_matches < 2)
                        {
                            continue;
                        }
                        else
                        {
                            SpawnBonus(y_neighbours[i].Position, ElementToCreate.Bomb, new Vector2(0, 0));
                            DeleteMatchElements(new_right_matches, new_left_matches, new_indexOf_X, new_x_neighbours);

                        }
                    }

                    if (top_matches + bot_matches == 3)
                    {
                        SpawnBonus(this.Position, ElementToCreate.LineBonus, new Vector2(0, 1));
                    }
                    else if (top_matches + bot_matches > 3)
                    {
                        SpawnBonus(this.Position, ElementToCreate.Bomb, new Vector2(0, 0));
                    }
                    
                }

                //Обязательная подчистка всего остального
                if (right_matches + left_matches >= 2)
                {
                    DeleteMatchElements(right_matches, left_matches, indexOf_X, x_neighbours);
                }
                if (top_matches + bot_matches >= 2)
                {
                    DeleteMatchElements(bot_matches, top_matches, indexOf_Y, y_neighbours);
                }

                //SpawnBombsDeleteMatches(sprites, ref indexOf_X, x_neighbours, ref right_matches, left_matches, new Vector2(1, 0));
                //SpawnBombsDeleteMatches(sprites, ref indexOf_Y, y_neighbours, ref bot_matches, top_matches, new Vector2(0, 1));
            }
        }

        private void DeleteMatchElements(int right_matches, int left_matches, int indexOf, List<Sprite> sprites)
        {
            right_matches += indexOf;
            indexOf -= left_matches;
            while (indexOf <= right_matches)
            {
                sprites[indexOf].IsRemoved = true;
                indexOf++;
            }
        }
        //Добавляет в стэш спавнера бонус-элемент
        private void SpawnBonus(Vector2 spawnPosition, ElementToCreate elementToCreate, Vector2 lineBonusDest)
        {
            switch (elementToCreate)
            {
                case ElementToCreate.LineBonus:
                    Texture2D texture;
                    if (lineBonusDest.X == 1)
                        texture = MainGameState.LinePlanetTextures[(int)this.planetType];
                    else
                        texture = MainGameState.LinePlanetTextures[(int)this.planetType + 5];
                    LineBonus temp_sprite = new LineBonus(texture)
                    {
                        planetType = this.planetType,
                        Position = spawnPosition,
                        BonusDirection = lineBonusDest
                    };
                    MainGameState.spriteSpawner.AddBonusToStash(temp_sprite);
                    break;

                case ElementToCreate.Bomb:
                    Bomb bomb = new Bomb(MainGameState.BombPlanetTextures[(int)this.planetType])
                    {
                        Position = spawnPosition,
                        planetType = this.planetType
                    };
                    MainGameState.spriteSpawner.AddBonusToStash(bomb);
                    break;
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
