using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1.Effects;
using System;
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
        private int _gametimeLeft = game_length;
        private int _score = 0;
        private int _uniqueElementsCount = 5;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        //Размеры окна
        public static Random BlessRNG = new Random();

        public static int ScreenWidth;
        public static int ScreenHeight;
        public static GameGrid gameGrid;

        private Texture2D _texture;
        private Texture2D[] _planetTextures;
        private List<Sprite> _sprites;
        private int[] _spriteSpawner;
        private Sprite[,] _spriteMatrix;
        

        public Planet test_planet;
        public Vector2 test_vector;

        

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

            test_vector = new Vector2(25, 25);
            gameGrid = new GameGrid(50);
            gameGrid.SetLocation(new Vector2((_graphics.GraphicsDevice.PresentationParameters.Bounds.Width - gameGrid.Width) / 2, 40));
            _spriteSpawner = new int[gameGrid.GridSize];
            _planetTextures = new Texture2D[_uniqueElementsCount];

            _sprites = new List<Sprite>();
            _spriteMatrix = new Sprite[gameGrid.GridSize, gameGrid.GridSize];

            
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _texture = Content.Load<Texture2D>("lil_hole");
            LoadPlanets();
            test_planet = new Planet(_texture)
            {
                Position = test_vector
            };
            gameGrid.SetTexture(Content.Load<Texture2D>("cosmos_background"));



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
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();
            //Point mouse_coord = Mouse.GetState().Position;
            //Rectangle sprite_rec = new Rectangle(test_planet.Position.ToPoint(), new Point(_texture.Width, _texture.Height));
            //test_planet.Update(gameTime, );


            SpawnNewPlanets();


            //Основной метод update для планет (гравитация)
            foreach (var pl in _sprites)
            {
                var bottom_sprites = _sprites.Where(sprite => sprite.rectangle.Left == pl.rectangle.Left && sprite.rectangle.Top > pl.rectangle.Bottom).Select(x => x).ToList();
                pl.Update(gameTime, bottom_sprites);
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Point mouse_coord = Mouse.GetState().Position;
                Sprite result_sprite = _sprites.Where(sp => sp.rectangle.Contains(mouse_coord)).Select(x => x).FirstOrDefault();
                //List<Sprite> res = _sprites.Where(sp => Math.Abs(sp.Position.X - result_sprite.Position.X) == 54 |
                //Math.Abs(sp.Position.Y - result_sprite.Position.Y) == 54).Select(x => x).ToList();
                List<Sprite> test = new List<Sprite>();
                float def_x = 0;
                float def_y = 0;
                if (result_sprite != null)
                {
                    foreach (Sprite sp in _sprites)
                    {
                        def_x = Math.Abs(sp.Position.X - result_sprite.Position.X);
                        def_y = Math.Abs(sp.Position.Y - result_sprite.Position.Y);
                        if ((def_x == 54 && sp.Position.Y == result_sprite.Position.Y) | (def_y == 54 && sp.Position.X == result_sprite.Position.X))
                        {
                            test.Add(sp);
                        }
                    }
                    result_sprite.IsRemoved = true;
                    foreach (Sprite t in test)
                    {
                        t.IsRemoved = true;
                    }
                }
            }

            //Заполнение матрицы и проверка матчей!
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                _spriteMatrix = FillSpriteMatrix();
                var res =  ScanMatrixForNulls();
                if (res != false)
                {
                    ScanMatrixForMatches();
                }

            }

            PostUpdate();

            base.Update(gameTime);
        }
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
                var x_pos = gameGrid.GetXLocationIndex(sp.Position);
                var y_pos = gameGrid.GetYLocationIndex(sp.Position);
                if (x_pos != -1 && y_pos != -1)
                    result_arr[x_pos, y_pos] = sp;
            }

            return result_arr;
        }

        private int GetMatchNumbers( (int,int) destination, int i ,int j, Sprite[,] arr)
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
            //try
            //{
            //    if (curr_elem.planetType != next_elem.planetType)
            //    {
            //        return 0;
            //    }
            //}
            //catch
            //{
            //    return 0;
            //}
            if (curr_elem.planetType != next_elem.planetType)
            {
                return 0;
            }


            if (curr_elem.planetType == next_elem.planetType)
            {
                count += 1 + GetMatchNumbers(destination, i + destination.Item1, j + destination.Item2, arr);
            }
            return count;
        }




        //Respawn new planets
        private void SpawnNewPlanets()
        {
            int plan_type;
            Planet new_sprite;
            float start_coodr = gameGrid.Location.Y - 100;
            for (int i = 0; i < _spriteSpawner.Length; i++)
            {
                while (_spriteSpawner[i] > 0)
                {
                    plan_type = BlessRNG.Next(0, 5);
                    new_sprite = new Planet(_planetTextures[plan_type])
                    {
                        Position = new Vector2(gameGrid.SpriteLocations[0, i].X, start_coodr),
                        planetType = (PlanetType)plan_type
                    };
                    new_sprite.SpawnCollision(_sprites);
                    _sprites.Add(new_sprite);
                    _spriteSpawner[i]--;
                }
            }
        }

        //Уборка уничтоженных спрайтов
        //и заполнение спавнера!
        private void PostUpdate()
        {
            int i = 0;
            while (i < _sprites.Count)
            {
                if (_sprites[i].IsRemoved == true)
                {
                    int res = gameGrid.GetXLocationIndex(_sprites[i].Position);
                    _spriteSpawner[res] += 1;
                    //TEST
                    //_sprites.RemoveAt(i + 1);
                    //_spriteSpawner[gameGrid.GetLocationIndex(_sprites[i + 1].Position)] += 1;
                    //TEST
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
            gameGrid.Draw(_spriteBatch);   
            test_planet.Draw(_spriteBatch);
            foreach (var pl in _sprites)
            {
                pl.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
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


    }
}
