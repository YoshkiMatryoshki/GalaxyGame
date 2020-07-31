using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;
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
        public int CellSize;
        public float BottomLine;

        public Vector2 Location; //top left corner
        public Vector2[,] SpriteLocations; //Тут информация о "разрешенных" позициях всех спрайтов

        public int GridSize {get;} = 8;
        public int BorderSize { get; } = 7;
        private Texture2D _texture;
        private Rectangle _resizeTextureRec;

        public GameGrid(int elem_texture_edge)
        {
            //Ширина поля с учетом всех границ
            Width = BorderSize * (GridSize + 1) + (GridSize * elem_texture_edge);
            Height = Width;
            CellSize = elem_texture_edge;
            
       

        }
        //Заполнение координат
        private void FillCoords()
        {
            SpriteLocations = new Vector2[GridSize, GridSize];
            float x_pos = Location.X;
            float y_pos = Location.Y;
            //Заполнение координатной сетки
            for (int i = 0; i < GridSize; i++)
            {
                y_pos += BorderSize;
                x_pos += BorderSize;
                for (int j = 0; j < GridSize; j++)
                {       
                    SpriteLocations[i, j] = new Vector2(x_pos, y_pos);
                    x_pos += CellSize + BorderSize;
                }
                y_pos += CellSize;
                x_pos = Location.X;
            }
        }

        public void SetLocation(Vector2 location)
        {
            Location = location;
            BottomLine = Location.Y + Height;
            FillCoords();
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
