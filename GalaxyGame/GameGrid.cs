using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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
        public int BorderSize { get; } = 4;
        private Texture2D _texture;
        private Rectangle _resizeTextureRec;
        private int[,] matrix;

        public GameGrid(int elem_texture_edge)
        {
            //Ширина поля с учетом всех границ
            Width = BorderSize * (GridSize + 1) + (GridSize * elem_texture_edge);
            Height = Width;
            CellSize = elem_texture_edge;
            matrix = new int[GridSize, GridSize];
            ClearMatrix();
       

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
        //Get index in SpriteLocations
        internal int GetXLocationIndex(Vector2 sprite_pos)
        {
            int i = 0;
            while (i < GridSize)
            {
                if (sprite_pos.X == SpriteLocations[0, i].X)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
        internal int GetYLocationIndex(Vector2 sprite_pos)
        {
            int i = 0;
            while (i < GridSize)
            {
                if (sprite_pos.Y == SpriteLocations[i, 0].Y)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
        internal Point GetXYLocationIndexes(Vector2 sprite_pos)
        {
            Point res = new Point(-1, -1);

            int i = 0;
            while (i< GridSize)
            {
                if (sprite_pos.X == SpriteLocations[0, i].X)
                {
                    res.Y = i; //ВОТ ТУТ ВНИМАТЕЛЬНО!!! X_Y Свапаются. бородатая затея бзв
                    i = GridSize;
                }
                i++;
            }
            i = 0;
            while(i < GridSize)
            {
                if (sprite_pos.Y == SpriteLocations[i, 0].Y)
                {
                    res.X = i;  //ВОТ ТУТ ВНИМАТЕЛЬНО!!! X_Y Свапаются. бородатая затея бзв
                    i = GridSize;
                }
                i++;
            }

            return res;
        }




        #region CheckMatchesNaMinimalkax
        public void FillCheckMatrix(List<Sprite> sprites)
        {
            List<Sprite> cleared_sprites = sprites.Where(sp => sp.GetType() != typeof(Destroyer)).Select(x => x).ToList();
            Point position;
            Planet pl;
            foreach(Sprite sp in sprites)
            {
                pl = sp as Planet;
                position = GetXYLocationIndexes(sp.Position);
                if (position.X != -1 && position.Y != -1)
                {
                    matrix[position.X, position.Y] = (int)pl.planetType;
                }     
            }
        }
        //Проверка матрицы на наличие хотя бы одного матча
        public bool IsThereAMatch()
        {
            bool res = false;
            int i = 0;
            while (i< GridSize && res == false)
            {
                int j = 0;
                while (j < GridSize)
                {
                    int match = GetMatchNumbers((0, 1), i, j,matrix);
                    if (match >= 2)
                        return true;
                    else
                        j++;
                }
                i++;
            }
            //По вертикали
            i = 0;
            while (i < GridSize && res == false)
            {
                int j = 0;
                while (j < GridSize)
                {
                    int match = GetMatchNumbers((1, 0), i, j, matrix);
                    if (match >= 2)
                        return true;
                    else
                        j++;
                }
                i++;
            }


            return false;
        }


        public void ClearMatrix()
        {
            for (int i = 0; i< matrix.GetUpperBound(0); i++)
            {
                for(int j = 0; j < matrix.GetUpperBound(0); j++)
                {
                    matrix[i, j] = -1;
                }
            }
        }



        private int GetMatchNumbers((int, int) destination, int i, int j, int[,] matrix)
        {
            int count = 0;
            int current = matrix[i, j];
            int next;
            try
            {
                next= matrix[i + destination.Item1, j + destination.Item2];
            }
            catch
            {
                return 0;
            }
            if (current != next)
            {
                return 0;
            }
            else
            {
                return 1 + GetMatchNumbers(destination, i + destination.Item1, j + destination.Item2, matrix);
            }
        }

        #endregion
    }
}
