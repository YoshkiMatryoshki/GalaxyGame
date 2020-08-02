using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1.Effects;
using SharpDX.MediaFoundation;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyGame
{
    public enum PlanetType
    {
        Earth = 0,
        Neptune = 1,
        Mars = 2,
        Saturn = 3,
        Asteroid = 4
    }
    public class Game1 : Game
    {
        //Основные параметры игры
        const int game_length = 60;
        private bool _gameStarted = false;
        private int _uniqueElementsCount = 5;
        private float _timer;
        private float _collisionTimer;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameInfo _gameInfo;


        //Размеры окна
        public static Random BlessRNG = new Random();


        public static GameGrid gameGrid;
        public static SpriteSpawner spriteSpawner;
        public static bool IsElementClicked = false;
        private static List<Sprite> _backTextures; 
        private ButtonState _previousState;
        private ButtonState _currentState;



        private Texture2D[] _planetTextures;
        public static Texture2D[] LinePlanetTextures;
        public static Texture2D BombTexture;
        private List<Sprite> _sprites;

        private Sprite[,] _spriteMatrix;


        public static Planet CurrentClickedPlanet;
        public static Planet SecondPlanet;
        
    
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight += 60;
            _graphics.ApplyChanges();

            gameGrid = new GameGrid(50);
            gameGrid.SetLocation(new Vector2((_graphics.GraphicsDevice.PresentationParameters.Bounds.Width - gameGrid.Width) - 50, 40));
            spriteSpawner = new SpriteSpawner(gameGrid.GridSize);

            _planetTextures = new Texture2D[_uniqueElementsCount];
            LinePlanetTextures = new Texture2D[_uniqueElementsCount * 2];

            _sprites = new List<Sprite>();
            _backTextures = new List<Sprite>();
            
            _spriteMatrix = new Sprite[gameGrid.GridSize, gameGrid.GridSize];

            _collisionTimer = 0;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            BombTexture = Content.Load<Texture2D>("OtherElements/black");
            _backTextures = new List<Sprite>()
            {
                new Sprite(Content.Load<Texture2D>("Background/fon"))
                {
                     Position = new Vector2(0, 0)
                },
                new Sprite(Content.Load<Texture2D>("Background/gameinfo"))
                {
                    Position = new Vector2(20,40)
                }
            };
            _gameInfo = new GameInfo(Content.Load<SpriteFont>("Fonts/GameInfo"), _backTextures[1].Position)
            {
                GameTime = game_length,
                Score = 0
            };


            LoadPlanets();
            LoadLineBonuses();

            gameGrid.SetTexture(Content.Load<Texture2D>("Background/podkladka"));
            LineBonus.Destroyer = new Destroyer(Content.Load<Texture2D>("OtherElements/destroyer"));


            int plan_type;
            for (int i = 0; i< gameGrid.GridSize; i++)
            {
                for (int j = 0; j < gameGrid.GridSize; j++)
                {
                    plan_type = BlessRNG.Next(0, 5);
                    _sprites.Add(new Planet(_planetTextures[plan_type])
                    {
                        Position = new Vector2(gameGrid.SpriteLocations[i,j].X, gameGrid.SpriteLocations[i,j].Y),
                        planetType = (PlanetType)plan_type
                    });
                }
                
            }


        }


        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                _gameStarted = true;
                _gameInfo.GameTime = game_length;
                _gameInfo.Score = 0;
            }
                
            if (!_gameStarted)
                return;
            if (_timer >= 1f)
            {
                _gameInfo.GameTime -= (int)_timer;
                _timer = 0;
            }
            if (_gameInfo.GameTime <= 0)
                _gameStarted = false;


            _previousState = _currentState;
            _currentState = Mouse.GetState().LeftButton;

            SpawnNewPlanets();
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _collisionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Основной метод update для планет (гравитация)
            //Выключено, пока элемент вращается
            foreach (var pl in _sprites)
            {
                pl.Update(gameTime, _sprites);
            }
            //Проверка матчей и удаление совпадающих элементов
            if (_collisionTimer > 1f)
            {
                foreach(var pl in _sprites.ToArray())
                {
                    pl.MatchDetection(gameTime, _sprites);
                }
                _collisionTimer = 0;
            }


            if (ScanMatrixForNulls() || !ScanMatrixForNulls())
            {
                SwapClickWorks();
            }

            PostUpdate(gameTime);

            base.Update(gameTime);
        }



        //Уборка уничтоженных спрайтов
        //и заполнение спавнера!
        private void PostUpdate(GameTime gameTime)
        {
            int i = 0;
            while (i < _sprites.Count)
            {
                if (_sprites[i].IsRemoved == true)
                {
                    int res = BlessRNG.Next(0, 5);
                    if (_sprites[i].GetType() == typeof(LineBonus))
                    {
                        _sprites[i].Update(null, _sprites);
                        spriteSpawner.AddPlanet(_sprites[i].Position, _planetTextures[res], res);
                        _gameInfo.Score += 1;
                    }
                    if (_sprites[i].GetType() == typeof(Planet))
                    {
                        //int res = gameGrid.GetXLocationIndex(_sprites[i].Position);
                        //_spriteSpawner[res] += 1;
                        spriteSpawner.AddPlanet(_sprites[i].Position, _planetTextures[res], res);
                        _gameInfo.Score += 1;
                    }
                    if (_sprites[i].GetType() == typeof(Bomb))
                    {
                        _sprites[i].Update(gameTime, _sprites);
                        spriteSpawner.AddPlanet(_sprites[i].Position, _planetTextures[res], res);
                    }
                    _sprites.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            foreach (Sprite texture in _backTextures)
            {
                texture.Draw(_spriteBatch);
            }
            gameGrid.Draw(_spriteBatch);
            foreach (var pl in _sprites)
            {
                pl.Draw(_spriteBatch);
            }
            _gameInfo.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }






        private void SwapClickWorks()
        {
            if (_currentState == ButtonState.Pressed && _previousState == ButtonState.Released
                && CurrentClickedPlanet != null && SecondPlanet != null)
            {
                CurrentClickedPlanet.MoveToOriginPlace();
                if (CheckIfNeighbours(SecondPlanet))
                {
                    CurrentClickedPlanet.Destinaition = SecondPlanet.Position;
                    SecondPlanet.Destinaition = CurrentClickedPlanet.Position;
                }
                else
                {
                    CurrentClickedPlanet = null;
                    SecondPlanet = null;
                    IsElementClicked = false;
                }
            }
            if (SecondPlanet != null && CurrentClickedPlanet != null && CurrentClickedPlanet.Position == CurrentClickedPlanet.Destinaition
                && SecondPlanet.Position == SecondPlanet.Destinaition)
            {

                Point main_loc = gameGrid.GetXYLocationIndexes(CurrentClickedPlanet.Position);
                Point sec_loc = gameGrid.GetXYLocationIndexes(SecondPlanet.Position);

                _spriteMatrix[main_loc.X, main_loc.Y] = CurrentClickedPlanet;
                _spriteMatrix[sec_loc.X, sec_loc.Y] = SecondPlanet;


                int horizontal_match = GetMatchNumbers((1, 0), main_loc.X, main_loc.Y, _spriteMatrix) + GetMatchNumbers((-1, 0), main_loc.X, main_loc.Y, _spriteMatrix);
                int vertical_match = GetMatchNumbers((0, 1), main_loc.X, main_loc.Y, _spriteMatrix) + GetMatchNumbers((0, -1), main_loc.X, main_loc.Y, _spriteMatrix);
                if (horizontal_match < 2 && vertical_match < 2)
                {
                    horizontal_match = GetMatchNumbers((1, 0), sec_loc.X, sec_loc.Y, _spriteMatrix) + GetMatchNumbers((-1, 0), sec_loc.X, sec_loc.Y, _spriteMatrix);
                    vertical_match = GetMatchNumbers((0, 1), sec_loc.X, sec_loc.Y, _spriteMatrix) + GetMatchNumbers((0, -1), sec_loc.X, sec_loc.Y, _spriteMatrix);
                }

                //if (horizontal_match < 2 && vertical_match < 2)
                //{
                //    CurrentClickedPlanet.Destinaition = SecondPlanet.Position;
                //    SecondPlanet.Destinaition = CurrentClickedPlanet.Position;
                //}
                CurrentClickedPlanet = null;
                SecondPlanet = null;
                IsElementClicked = false;
            }
        }

        //Проверка, являются ли выделенные кликнутые элементы соседями
        private static bool CheckIfNeighbours(Sprite second_planet)
        {
            bool res = false;
            if (Math.Abs(CurrentClickedPlanet.Origin.X - second_planet.Origin.X) <= gameGrid.CellSize + gameGrid.BorderSize + 5
                && CurrentClickedPlanet.Origin.Y == second_planet.Origin.Y)
            {
                res = true;
            }
            if (Math.Abs(CurrentClickedPlanet.Origin.Y - second_planet.Origin.Y) <= gameGrid.CellSize + gameGrid.BorderSize + 5
                && CurrentClickedPlanet.Origin.X == second_planet.Origin.X)
            {
                res = true;
            }

            return res;
        }





        //Respawn new planets
        private void SpawnNewPlanets()
        {
            //int plan_type;
            //Planet new_sprite;
            //float start_coodr = gameGrid.Location.Y - 100;
            //for (int i = 0; i < _spriteSpawner.Length; i++)
            //{
            //    while (_spriteSpawner[i] > 0)
            //    {
            //        plan_type = BlessRNG.Next(0, 5);
            //        new_sprite = new Planet(_planetTextures[plan_type])
            //        {
            //            Position = new Vector2(gameGrid.SpriteLocations[0, i].X, start_coodr),
            //            planetType = (PlanetType)plan_type
            //        };
            //        new_sprite.SpawnCollision(_sprites);
            //        _sprites.Add(new_sprite);
            //        _spriteSpawner[i]--;
            //    }
            //}
            spriteSpawner.SpawnAll(_sprites);



        }



        //Загружает спрайты всех планеток.
        private void LoadPlanets()
        {
            _planetTextures[0] = Content.Load<Texture2D>("Planets/earth");
            _planetTextures[1] = Content.Load<Texture2D>("Planets/Neptune");
            _planetTextures[2] = Content.Load<Texture2D>("Planets/Mars");
            _planetTextures[3] = Content.Load<Texture2D>("Planets/Saturn");
            _planetTextures[4] = Content.Load<Texture2D>("Planets/Asteroid");
        }
        //ПОдгрузка line бонусов
        private void LoadLineBonuses()
        {
            LinePlanetTextures[0] = Content.Load<Texture2D>("LineBonuses/earth_line_horizontal");
            LinePlanetTextures[1] = Content.Load<Texture2D>("LineBonuses/neptune_line_horizontal");
            LinePlanetTextures[2] = Content.Load<Texture2D>("LineBonuses/mars_line_horizontal");
            LinePlanetTextures[3] = Content.Load<Texture2D>("LineBonuses/saturn_line_horizontal");
            LinePlanetTextures[4] = Content.Load<Texture2D>("LineBonuses/asteroid_line_horizontal");
            LinePlanetTextures[5] = Content.Load<Texture2D>("LineBonuses/earth_line_vertical");
            LinePlanetTextures[6] = Content.Load<Texture2D>("LineBonuses/neptune_line_vertical");
            LinePlanetTextures[7] = Content.Load<Texture2D>("LineBonuses/mars_line_vertical");
            LinePlanetTextures[8] = Content.Load<Texture2D>("LineBonuses/saturn_line_vertical");
            LinePlanetTextures[9] = Content.Load<Texture2D>("LineBonuses/asteroid_line_vertical");

        }

        #region Oblilette старые идеи

        //Проверка, полностью ли заполнена матрица
        //return true if wegucci
        private bool ScanMatrixForNulls()
        {
            var res = _spriteMatrix.GetEnumerator();
            res.Reset();
            while (res.MoveNext())
            {
                if (res.Current == null)
                {
                    return false;
                }
            }
            return true;
        }

        //Поиск совпадающих элементов
        private void ScanMatrixForMatches()
        {
            //проход таблицы по горизонтали
            for (int j = 0; j < gameGrid.GridSize; j++)
            {
                int i = 0;
                while (i < gameGrid.GridSize)
                {
                    int res = GetMatchNumbers((1, 0), i, j, _spriteMatrix);
                    if (res == 2)
                    {
                        int tp = BlessRNG.Next(0, 5);
                        _sprites.Add(new LineBonus(LinePlanetTextures[tp])
                        {
                            planetType = (PlanetType)tp,
                            Position = new Vector2(_spriteMatrix[i + BlessRNG.Next(0, res + 1), j].Position.X, gameGrid.Location.Y - 100),
                            //Destroyer = new Destroyer(_texture),
                            BonusDirection = new Vector2(1, 0)
                        });
                    }
                    if (res >= 2)
                    {
                        while (res >= 0)
                        {
                            _spriteMatrix[i, j].IsRemoved = true;
                            i++;
                            res--;
                        }
                    }
                    else
                    {
                        i++;
                    }

                }
            }
            //проход таблицы по вертикали
            for (int i = 0; i < gameGrid.GridSize; i++)
            {
                int j = 0;
                while (j < gameGrid.GridSize)
                {
                    int res = GetMatchNumbers((0, 1), i, j, _spriteMatrix);
                    if (res >= 2)
                    {
                        while (res >= 0)
                        {
                            _spriteMatrix[i, j].IsRemoved = true;
                            j++;
                            res--;
                        }
                    }
                    else
                    {
                        j++;
                    }

                }
            }
        }
        //Заполняет матрицу спрайтов, не поверите, спрайтами на основе их Position.
        private Sprite[,] FillSpriteMatrix()
        {
            Sprite[,] result_arr = new Sprite[gameGrid.GridSize, gameGrid.GridSize];
            foreach (Sprite sp in _sprites)
            {
                //var x_pos = gameGrid.GetXLocationIndex(sp.Position);
                //var y_pos = gameGrid.GetYLocationIndex(sp.Position);
                var res_point = gameGrid.GetXYLocationIndexes(sp.Position);
                if (res_point.X != -1 && res_point.Y != -1)
                    result_arr[res_point.X, res_point.Y] = sp;
            }

            return result_arr;
        }
        private int GetMatchNumbers((int, int) destination, int i, int j, Sprite[,] arr)
        {
            int count = 0;
            Planet curr_elem = arr[i, j] as Planet;
            Planet next_elem;
            try
            {
                next_elem = arr[i + destination.Item1, j + destination.Item2] as Planet;
            }
            catch
            {
                return 0;
            }
            try
            {
                if (curr_elem.planetType != next_elem.planetType)
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
            //if (curr_elem.planetType != next_elem.planetType)
            //{
            //    return 0;
            //}


            if (curr_elem.planetType == next_elem.planetType)
            {
                count += 1 + GetMatchNumbers(destination, i + destination.Item1, j + destination.Item2, arr);
            }
            return count;
        }


        //Заполнение матрицы и проверка матчей!
        //Keyboard.GetState().IsKeyDown(Keys.Space) &&
        //if (_timer > 1f && IsElementClicked == false)
        //{
        //    _spriteMatrix = FillSpriteMatrix();
        //    ScanMatrixForMatches();
        //    //var res = ScanMatrixForNulls();
        //    //if (res != false)
        //    //{
        //    //    ScanMatrixForMatches();
        //    //}
        //    _timer = 0;
        //}
        #endregion
    }
}
