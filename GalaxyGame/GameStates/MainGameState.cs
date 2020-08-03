using GalaxyGame.AnimationWorks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalaxyGame.GameStates
{
    public class MainGameState : GameState
    {
        const int game_length = 60;
        const int _uniqueElementsCount = 5;
        private bool _gameOver = false;

        private float _timer;
        private float _collisionTimer;
        private float _omegalulTimer;

        private GameInfo _gameInfo;

        public static Random BlessRNG = new Random();
        public static bool FieldHasNoMatches = false;
        public static GameGrid gameGrid;
        public static SpriteSpawner spriteSpawner;
        public static bool FreezeField = false;
        private static List<Sprite> _backTextures;
        private ButtonState _previousState;
        private ButtonState _currentState;

        private Texture2D[] _planetTextures;
        public static Texture2D[] LinePlanetTextures;
        public static Texture2D BombTexture;
        public static Texture2D ExplosionTexture;
        private List<Sprite> _sprites;
        public static List<Animation> _explosions;
        

        public static Planet CurrentClickedPlanet;
        public static Planet SecondPlanet;


        private InfoMessage GameOverMsg;
        


        public MainGameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content) : base(game, graphicsDevice, content)
        {
            gameGrid = new GameGrid(50);
            gameGrid.SetLocation(new Vector2((graphicsDevice.Viewport.Width - gameGrid.Width) - 50, 40));

            spriteSpawner = new SpriteSpawner(gameGrid.GridSize);
            _planetTextures = new Texture2D[_uniqueElementsCount];
            LinePlanetTextures = new Texture2D[_uniqueElementsCount * 2];

            _sprites = new List<Sprite>();
            _backTextures = new List<Sprite>();


            ExplosionTexture =  _content.Load<Texture2D>("Animations/Explosion");
            _explosions = new List<Animation>();
            //_collisionTimer = 0;
            LoadInfoMessage();
            LoadContent();

            Restart();

        }



        public override void Update(GameTime gameTime)
        {


            if (GameOverMsg.Accepted || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _mainGame._nextGameState = _mainGame.Menu;
                Restart();
            }
            if (_gameInfo.GameTime <= 0)
            {
                _gameOver = true;
                GameOverMsg.Update();
                return;       
            }

            if (_timer >= 1f)
            {
                _gameInfo.GameTime -= (int)_timer;
                _timer = 0;
            }

            _previousState = _currentState;
            _currentState = Mouse.GetState().LeftButton;

            spriteSpawner.SpawnAll(_sprites);
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _collisionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _omegalulTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Sprite anyDesters = _sprites.Where(sp => sp is Destroyer).FirstOrDefault();

            if (anyDesters == null && _explosions.Count == 0)
            {
                FreezeField = false;
            }

            //Основной метод update для планет (гравитация)
            foreach (var pl in _sprites)
            {
                pl.Update(gameTime, _sprites);
            }
            foreach(Animation exp in _explosions)
            {
                exp.Update(gameTime);
            }


            if (!CheckRespawned(_sprites) && _omegalulTimer > 1f)
            {
                gameGrid.FillCheckMatrix(_sprites);
                var res = gameGrid.IsThereAMatch();
                if (res == false)
                {
                    FieldHasNoMatches = true;
                }
                _omegalulTimer = 0;

            }
            if (FieldHasNoMatches)
            {
                foreach (var sp in _sprites)
                {
                    if (sp.GetType() != typeof(Destroyer))
                        sp.FishForClick();

                }
                SwapClickWorks();
            }


            if (FieldHasNoMatches == false && _collisionTimer > 1f)
            {
                foreach (var pl in _sprites.ToArray())
                {
                    pl.MatchDetection(gameTime, _sprites);
                }
                _collisionTimer = 0;

            }

        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();


            spriteBatch.Draw(BackGroundTexture, new Vector2(0, 0), Color.White);
            gameGrid.Draw(spriteBatch);
            foreach (var pl in _sprites)
            {
                pl.Draw(spriteBatch);
            }
            foreach (var exp in _explosions)
            {
                exp.Draw(spriteBatch);
            }
            _backTextures[0].Draw(spriteBatch);
            _gameInfo.Draw(spriteBatch);


            if (_gameOver)
            {
                GameOverMsg.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
        //Уборка уничтоженных спрайтов
        //и заполнение спавнера!
        public override void PostUpdate(GameTime gameTime)
        {
            int i = 0;
            while (i < _sprites.Count)
            {
                if (_sprites[i].IsRemoved == true)
                {
                    try
                    {
                        Planet pl = _sprites[i] as Planet;
                        _explosions.Add(new Animation(ExplosionTexture, 6, pl.Position));
                        FreezeField = true;
                    }
                    catch
                    {

                    }
                    int res = BlessRNG.Next(0, 5);
                    if (_sprites[i].GetType() == typeof(LineBonus))
                    {
                        _sprites[i].Update(null, _sprites);
                        spriteSpawner.AddPlanet(_sprites[i].Position, _planetTextures[res], res);
                        _gameInfo.Score += 1;
                    }
                    if (_sprites[i].GetType() == typeof(Planet))
                    {
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
            int j = 0;
            while(j < _explosions.Count)
            {
                if (_explosions[j].HasEnded == true)
                {
                    _explosions.RemoveAt(j);
                    
                }
                else
                {
                    j++;
                }
                
            }
        }

        private void LoadContent()
        {
            BombTexture = _content.Load<Texture2D>("OtherElements/black");
            _backTextures = new List<Sprite>()
            {
                new Sprite(_content.Load<Texture2D>("Background/gameinfo"))
                {
                    Position = new Vector2(20,40)
                }
            };
            _gameInfo = new GameInfo(_content.Load<SpriteFont>("Fonts/GameInfo"), _backTextures[0].Position)
            {
                GameTime = game_length,
                Score = 0
            };
            LoadPlanets();
            LoadLineBonuses();

            gameGrid.SetTexture(_content.Load<Texture2D>("Background/podkladka"));
            LineBonus.Destroyer = new Destroyer(_content.Load<Texture2D>("OtherElements/destroyer"));

        }

        private void Restart()
        {
            GameOverMsg.Accepted = false;
            _gameOver = false;
            FieldHasNoMatches = false;
            _gameInfo.GameTime = game_length;
            _gameInfo.Score = 0;
            _timer = 0;
            _collisionTimer = 0.8f;
            _omegalulTimer = 0.8f;
            int plan_type;
            _sprites.Clear();
            _explosions.Clear();
            for (int i = 0; i < gameGrid.GridSize; i++)
            {
                for (int j = 0; j < gameGrid.GridSize; j++)
                {
                    plan_type = BlessRNG.Next(0, 5);
                    _sprites.Add(new Planet(_planetTextures[plan_type])
                    {
                        Position = new Vector2(gameGrid.SpriteLocations[i, j].X, gameGrid.SpriteLocations[i, j].Y),
                        planetType = (PlanetType)plan_type,
                        ParentState = this
                    });
                }

            }
        }


        //Проверка, есть ли реснутые элементы, не достигшие своей позиции в таблице.
        private bool CheckRespawned(List<Sprite> sprites)
        {
            bool f = false;
            int i = 0;
            while (f == false && i < sprites.Count)
            {
                if (sprites[i].rectangle.Top < gameGrid.Location.Y + gameGrid.BorderSize)
                {
                    f = true;
                }
                i++;
            }
            return f;
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
                }
            }
            if (SecondPlanet != null && CurrentClickedPlanet != null && CurrentClickedPlanet.Position == CurrentClickedPlanet.Destinaition
                && SecondPlanet.Position == SecondPlanet.Destinaition)
            {
                CurrentClickedPlanet.MatchDetection(null, _sprites);
                SecondPlanet.MatchDetection(null, _sprites);
                if (CurrentClickedPlanet.IsRemoved == false && SecondPlanet.IsRemoved == false)
                {
                    CurrentClickedPlanet.Destinaition = SecondPlanet.Position;
                    SecondPlanet.Destinaition = CurrentClickedPlanet.Position;
                }
                else
                {
                    FieldHasNoMatches = false;
                }
                CurrentClickedPlanet = null;
                SecondPlanet = null;
            }
        }
        //Проверка, являются ли выделенные кликнутые элементы соседями
        private static bool CheckIfNeighbours(Sprite second_planet)
        {

            if (Math.Abs(CurrentClickedPlanet.Origin.X - second_planet.Origin.X) <= gameGrid.CellSize + gameGrid.BorderSize + 5
                && CurrentClickedPlanet.Origin.Y == second_planet.Origin.Y)
            {
                return true;
            }
            if (Math.Abs(CurrentClickedPlanet.Origin.Y - second_planet.Origin.Y) <= gameGrid.CellSize + gameGrid.BorderSize + 5
                && CurrentClickedPlanet.Origin.X == second_planet.Origin.X)
            {
                return true;
            }

            return false;
        }

        private void LoadPlanets()
        {
            _planetTextures[0] = _content.Load<Texture2D>("Planets/earth");
            _planetTextures[1] = _content.Load<Texture2D>("Planets/Neptune");
            _planetTextures[2] = _content.Load<Texture2D>("Planets/Mars");
            _planetTextures[3] = _content.Load<Texture2D>("Planets/Saturn");
            _planetTextures[4] = _content.Load<Texture2D>("Planets/Asteroid");
        }
        //ПОдгрузка line бонусов
        private void LoadLineBonuses()
        {
            LinePlanetTextures[0] = _content.Load<Texture2D>("LineBonuses/earth_line_horizontal");
            LinePlanetTextures[1] = _content.Load<Texture2D>("LineBonuses/neptune_line_horizontal");
            LinePlanetTextures[2] = _content.Load<Texture2D>("LineBonuses/mars_line_horizontal");
            LinePlanetTextures[3] = _content.Load<Texture2D>("LineBonuses/saturn_line_horizontal");
            LinePlanetTextures[4] = _content.Load<Texture2D>("LineBonuses/asteroid_line_horizontal");
            LinePlanetTextures[5] = _content.Load<Texture2D>("LineBonuses/earth_line_vertical");
            LinePlanetTextures[6] = _content.Load<Texture2D>("LineBonuses/neptune_line_vertical");
            LinePlanetTextures[7] = _content.Load<Texture2D>("LineBonuses/mars_line_vertical");
            LinePlanetTextures[8] = _content.Load<Texture2D>("LineBonuses/saturn_line_vertical");
            LinePlanetTextures[9] = _content.Load<Texture2D>("LineBonuses/asteroid_line_vertical");

        }

        private void LoadInfoMessage()
        {
            var msg_font = _content.Load<SpriteFont>("Fonts/MsgFont");
            var msg_button = new LonelyButton(_content.Load<Texture2D>("Background/LonelyButton"), _content.Load<SpriteFont>("Fonts/ButtonFont"));
            GameOverMsg = new InfoMessage(_content.Load<Texture2D>("Background/shittyborder"), msg_button, new Vector2(300, 100))
            {
                _headingFont = msg_font,
                Text = "Game over"
            };
        }
    }
}
