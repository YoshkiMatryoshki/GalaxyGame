using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GalaxyGame
{
    public class Game1 : Game
    {
        //Основные параметры игры
        const int game_length = 60;
        private int _gametimeLeft = game_length;
        private int _score = 0;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Random BlessRNG = new Random();

        public GameGrid gameGrid;

        private Texture2D _texture;

        public Planet test_planet;
        public Vector2 test_vector;
        

        public Planet[] planets = new Planet[7];

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            test_vector = new Vector2(100, 0);
            gameGrid = new GameGrid(376, new Vector2(150, 10));
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            _texture = Content.Load<Texture2D>("lil_hole");

            test_planet = new Planet(_texture, test_vector);


            for (int i = 0; i< planets.Length; i++)
            {
                planets[i] = new Planet(_texture, new Vector2(BlessRNG.Next(0, 400), BlessRNG.Next(0,300)));
            }


            //test_planet = new Planet(_texture, new Vector2(100, 200))
            //{
            //    planetType = PlanetType.Earth;
            //}
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();
            Point mouse_coord = Mouse.GetState().Position;
            Rectangle sprite_rec = new Rectangle(test_planet.Position.ToPoint(), new Point(_texture.Width, _texture.Height));
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && sprite_rec.Contains(mouse_coord))
            {
                if (test_planet.IsClicked == true)
                    test_planet.IsClicked = false;
                else
                    test_planet.IsClicked = true;
            }
            test_planet.Update();

            foreach(var pl in planets)
            {
                pl.Update();
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            test_planet.Draw(_spriteBatch);
            foreach (var pl in planets)
            {
                pl.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
