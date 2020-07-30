using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    //репрезентует сетку с ячейками, в которых будут расположены спрайты
    //На данном этапе поле всегда квадратное, но на будущее (ха - ха, бзв) оставлен простор для прямоугольных форм.
    public class GameGrid
    {
        public int Width;
        public int Height;
        public int cell_size;
        public float BottomLine;

        public Vector2 Location; //top left corner

        public int GridSize {get;} = 8;
        public int BorderSize { get; } = 10;
        private Texture2D _texture;
        private Rectangle _resizeTextureRec;

        public GameGrid(int gridedge)
        {
            Width = gridedge + BorderSize * (GridSize + 1);
            Height = Width;
            cell_size = gridedge / GridSize;
            
        }
        public void SetLocation(Vector2 location)
        {
            Location = location;
            BottomLine = Location.Y + Height;
        }
        public void SetTexture(Texture2D texture2D)
        {
            _texture = texture2D;
            _resizeTextureRec = new Rectangle(Location.ToPoint(), new Point(Width, Height));
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture,_resizeTextureRec, Color.White);
        }

    }
}
