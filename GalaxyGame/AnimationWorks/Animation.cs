using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame.AnimationWorks
{
    public class Animation
    {
        public int FrameCount { get; set; }
        public int Current { get; set; }
        public float AnimSpeed { get; set; }
        public bool HasEnded;

        public int FrameWidth;
        public int FrameHeight;

        private int VerticalMult;
        private int HorizontalMult;
        private float _timer;
        public Vector2 Position;

        public Texture2D Texture { get; private set; }

        public Animation(Texture2D texture, int frameCount, Vector2 position)
        {
            Position = position;
            Texture = texture;
            FrameCount = frameCount;
            HasEnded = false;
            AnimSpeed = 0.05f;
            VerticalMult = 0;
            HorizontalMult = 0;
            FrameWidth = 50;
            FrameHeight = 50;
        }

        public void Play()
        {
            Current = 0;
            _timer = 0;
        }
        public void Draw(SpriteBatch spriteBatch)
        {

            var anim_rect = new Rectangle(HorizontalMult * FrameWidth, FrameHeight * VerticalMult, FrameWidth, FrameHeight);
            spriteBatch.Draw(Texture, Position, anim_rect, Color.White);
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer > AnimSpeed)
            {
                _timer = 0;
                Current++;
                HorizontalMult++;

                if (HorizontalMult * 50 >= Texture.Width)
                {
                    VerticalMult++;
                    HorizontalMult = 0;
                    
                }
                if (Current >= FrameCount)
                {
                    //Current = 0;
                    //VerticalMult = 0;
                    HasEnded = true;
                }
            }

        }
    }
}
