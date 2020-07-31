using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static GameGrid gameGrid;

        private Texture2D _texture;
        private Texture2D[] _planetTextures;
        private List<Sprite> _sprites;

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
            _planetTextures = new Texture2D[_uniqueElementsCount];

            _sprites = new List<Sprite>();
            
            
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
            Point mouse_coord = Mouse.GetState().Position;
            Rectangle sprite_rec = new Rectangle(test_planet.Position.ToPoint(), new Point(_texture.Width, _texture.Height));



            //test_planet.Update(gameTime, );
            
            foreach (var pl in _sprites)
            {

                //List<Sprite> res = _sprites.Where(x => x.rectangle.Top == pl.rectangle.Bottom + Game1.gameGrid.BorderSize && x.rectangle.Left == pl.rectangle.Left).Select(x => x).ToList();
                //Sprite bottom_sprite = _sprites.FirstOrDefault(x => x.rectangle.Top == pl.rectangle.Bottom + gameGrid.BorderSize && x.rectangle.Left == pl.rectangle.Left);
                var bottom_sprites = _sprites.Where(sprite => sprite.rectangle.Left == pl.rectangle.Left && sprite.rectangle.Top > pl.rectangle.Bottom).Select(x => x).ToList();
                pl.Update(gameTime, bottom_sprites);  
            }

            //foreach(Planet pl in _sprites)
            //{
            //    pl.Update();
            //}

            PostUpdate();

            base.Update(gameTime);
        }

        //Уборка уничтоженных спрайтов
        private void PostUpdate()
        {
            int i = 0;
            while (i < _sprites.Count)
            {
                if (_sprites[i].IsRemoved == true)
                {
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
            _planetTextures[1] = Content.Load<Texture2D>("Planets/Venus");
            _planetTextures[2] = Content.Load<Texture2D>("Planets/Mars");
            _planetTextures[3] = Content.Load<Texture2D>("Planets/Saturn");
            _planetTextures[4] = Content.Load<Texture2D>("Planets/Satelite");
        }


    }
}
