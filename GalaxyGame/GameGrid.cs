using Microsoft.Xna.Framework;
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

        public Vector2 Location; //top left corner

        private const int _gridSize = 8;
        private const int _borderSize = 0;

        public GameGrid(int gridedge, Vector2 location)
        {
            Width = gridedge + _borderSize * (_gridSize - 1);
            Height = Width;
            Location = location;
            cell_size = Width / _gridSize;
        }

    }
}
