using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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

        public GameGrid gameGrid;

        private Texture2D _texture;
        private Texture2D[] _planetTextures;

        public Planet test_planet;
        public Vector2 test_vector;


        public Planet[,] planets;

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
            gameGrid = new GameGrid(376);
            gameGrid.SetLocation(new Vector2((_graphics.GraphicsDevice.PresentationParameters.Bounds.Width - gameGrid.Width) / 2, 40));
            _planetTextures = new Texture2D[_uniqueElementsCount];
            planets = new Planet[gameGrid.GridSize, gameGrid.GridSize];
            
            
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
            int sum = gameGrid.cell_size + gameGrid.BorderSize;
            int planet_x;
            int planet_y;
            for (int i = 0; i< gameGrid.GridSize; i++)
            {
                for (int j = 0; j < gameGrid.GridSize; j++)
                {
                    plan_type = BlessRNG.Next(0, 5);
                    planet_x = (int)(gameGrid.Location.X + gameGrid.BorderSize + j * sum);
                    planet_y = (int)(gameGrid.Location.Y + gameGrid.BorderSize + i * sum);

                    planets[i, j] = new Planet(_planetTextures[plan_type])
                    {
                        Position = new Vector2(planet_x, planet_y),
                        planetType = (PlanetType)plan_type
                    };
                }
                
            }

        }


        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();
            Point mouse_coord = Mouse.GetState().Position;
            Rectangle sprite_rec = new Rectangle(test_planet.Position.ToPoint(), new Point(_texture.Width, _texture.Height));

            

            //if (Mouse.GetState().LeftButton == ButtonState.Pressed && sprite_rec.Contains(mouse_coord))
            //{
            //    if (test_planet.IsClicked == true)
            //        test_planet.IsClicked = false;
            //    else
            //        test_planet.IsClicked = true;
            //}
            test_planet.Update(gameTime);


            //Проверка, если ли под элеентами ряда другие элементы или пустое пространство!
            //for(int i = gameGrid.GridSize-1; i >= 0; i--)
            //{
            //    for (int j = 0; j < gameGrid.GridSize; j++)
            //    {
                    
            //    }
            //}



            foreach(var pl in planets)
            {
                pl.Update(gameTime);
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            gameGrid.Draw(_spriteBatch);   
            test_planet.Draw(_spriteBatch);
            foreach (var pl in planets)
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
            _planetTextures[1] = Content.Load<Texture2D>("Planets/Venus");
            _planetTextures[2] = Content.Load<Texture2D>("Planets/Mars");
            _planetTextures[3] = Content.Load<Texture2D>("Planets/Saturn");
            _planetTextures[4] = Content.Load<Texture2D>("Planets/Satelite");
        }


    }
}
