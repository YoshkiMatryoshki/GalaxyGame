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
        private Planet[,] _planetMatrix;

        public GameGrid(int elem_texture_edge)
        {
            //Ширина поля с учетом всех границ
            Width = BorderSize * (GridSize + 1) + (GridSize * elem_texture_edge);
            Height = Width;
            CellSize = elem_texture_edge;
            _planetMatrix = new Planet[GridSize, GridSize];
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

        public void FillPlanetMatrix(List<Sprite> sprites)
        {
            Point position;
            foreach(Sprite sp in sprites)
            {
                position = GetXYLocationIndexes(sp.Position);
                if (position.X != -1 && position.Y != -1)
                {
                    _planetMatrix[position.X, position.Y] = sp as Planet;
                }
            }
        }
        /// <summary>
        /// Определяет матчи и возвращает набор бонусов для спавна
        /// </summary>
        public List<Planet> MatchDetection()
        {
            List<Planet> newBonuses = new List<Planet>();

            //Основная проверка по горизонтали с УЧЕТОМ ПЕРЕСЕЧЕНИЙ!!
            ElementToCreate temp_bonus = ElementToCreate.None;
            for(int i = 0; i < GridSize; i++)
            {
                for(int j = 0; j <GridSize; j++)
                {
                    //if already removed = skip
                    if (_planetMatrix[i,j].IsRemoved)
                    {
                        continue;
                    }
                    temp_bonus = ElementToCreate.None;
                    Planet spawnPlanet = _planetMatrix[i, j];
                    int right_match = GetMatchPlanets((0, 1), i, j);

                    temp_bonus = right_match switch
                    {
                        4 => ElementToCreate.Bomb,
                        3 => ElementToCreate.LineBonus,
                        _ => ElementToCreate.None
                    };

                    if (right_match >= 2)
                    {
                        for (int current = j; current <= right_match + j; current++)
                        {
                            int top_match = GetMatchPlanets((-1, 0), i, current);
                            int bot_match = GetMatchPlanets((1, 0), i, current);
                            if (top_match + bot_match >= 2)
                            {
                                temp_bonus = ElementToCreate.Bomb;
                                spawnPlanet = _planetMatrix[i, current];
                                RemovePlanets((1, 0), i - top_match, current, top_match + bot_match);
                            }
                        }
                        RemovePlanets((0, 1), i, j, right_match);
                    }
                    Planet bonus = newBonus(temp_bonus, spawnPlanet, new Vector2(1,0));
                    if (bonus != null)
                    {
                        newBonuses.Add(bonus);
                    }

                }
            }

            //Проверка по вертикали уже без учета пересечений
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (_planetMatrix[j, i].IsRemoved)
                    {
                        continue;
                    }
                    temp_bonus = ElementToCreate.None;
                    int bot_match = GetMatchPlanets((1, 0), j, i);
                    temp_bonus = bot_match switch
                    {
                        4 => ElementToCreate.Bomb,
                        3 => ElementToCreate.LineBonus,
                        _ => ElementToCreate.None
                    };
                    if (bot_match >= 2)
                    {
                        RemovePlanets((1, 0), j, i, bot_match);
                    }
                    //new vertical bonus
                    Planet bonus = newBonus(temp_bonus, _planetMatrix[j, i], new Vector2(0, 1));
                    if (bonus != null)
                    {
                        newBonuses.Add(bonus);
                    }
                }
            }
            return newBonuses;
        }


        //Создание элемента в зависимости от типа
        private Planet newBonus(ElementToCreate bonusType, Planet basePlanet, Vector2 dest) =>
            bonusType switch
            {
                ElementToCreate.Bomb => Bomb.CreateBombByPlanet(basePlanet),
                ElementToCreate.LineBonus => LineBonus.CreateBonusByPlanet(basePlanet, dest),
                ElementToCreate.None => null,
                _ => null
            };
        //Установка значений isRemoved = true;
        private void RemovePlanets((int,int) destination, int start_x, int start_y, int length)
        {
            for (int i = 0; i <= length; i++)
            {
                _planetMatrix[start_x, start_y].IsRemoved = true;
                start_x += destination.Item1;
                start_y += destination.Item2;
            }
        }

        //Проверка матрицы на наличие хотя бы одного матча
        public bool IsThereAMatch()
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int right_match = GetMatchPlanets((0, 1), i, j);
                    int bottom_match = GetMatchPlanets((1, 0), i, j);
                    if (right_match >= 2 || bottom_match >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public void ClearMatrix()
        {
            for (int i = 0; i< _planetMatrix.GetUpperBound(0); i++)
            {
                for(int j = 0; j < _planetMatrix.GetUpperBound(0); j++)
                {
                    _planetMatrix[i, j] = null;
                }
            }
        }



        private int GetMatchNumbers((int, int) destination, int i, int j, int[,] matrix)
        {
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

        private int GetMatchPlanets((int,int) destination, int pos_x, int pos_y)
        {
            Planet current;
            Planet next;
            int next_x = pos_x + destination.Item1;
            int next_y = pos_y + destination.Item2;
            //проверка на выход за пределы матрицы
            if (next_x >= GridSize || next_x < 0
                || next_y >= GridSize || next_y < 0)
            {
                return 0;
            }
            current = _planetMatrix[pos_x, pos_y];
            next = _planetMatrix[next_x, next_y];
            if (current.planetType != next.planetType)
            {
                return 0;
            }
            else
            {
                return 1 + GetMatchPlanets(destination, next_x, next_y);
            }
        }

        #endregion
    }
}
