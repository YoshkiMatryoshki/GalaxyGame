using GalaxyGame.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyGame
{
    public class SpriteSpawner
    {
        private static float _spawnY;

        private int[] _columnLocations;
        private List<Sprite> _spritesToSpawn;
        private Dictionary<Vector2, List<Sprite>> _spawnGrid;
        public SpriteSpawner(int columnCount)
        {
            _columnLocations = new int[columnCount];
            _spawnY = MainGameState.gameGrid.Location.Y - 100;
            _spritesToSpawn = new List<Sprite>();
            _spawnGrid = new Dictionary<Vector2, List<Sprite>>();
        }


        public void AddPlanet(Vector2 Pl_position, Texture2D texture, int planet_type_num)
        {
            var new_sprite = new Planet(texture)
            {
                Position = new Vector2(Pl_position.X, _spawnY),
                planetType = (PlanetType)planet_type_num
            };

            if (!_spawnGrid.ContainsKey(Pl_position))
            {
                _spawnGrid.Add(Pl_position, new List<Sprite>());
            }
            _spawnGrid[Pl_position].Add(new_sprite);

        }
        //Добавляет в спайт немножечко лайма или бомбы
        public void AddBonus(Sprite bonus, List<Sprite> main_sprite_list)
        {
            main_sprite_list.Add(bonus);
            int res = MainGameState.gameGrid.GetXLocationIndex(bonus.Position);
            _columnLocations[res] += 1;
        }

        public void SpawnAll(List<Sprite> main_sprite_list)
        {
            Planet pl;
            foreach(KeyValuePair<Vector2, List<Sprite>> column_resp in _spawnGrid)
            {
                int res = MainGameState.gameGrid.GetXLocationIndex(column_resp.Key);
                while(column_resp.Value.Count > 0)
                {
                    if (_columnLocations[res] == 0)
                    {
                        pl = (Planet)column_resp.Value[0];
                        pl.SpawnCollision(main_sprite_list);
                        main_sprite_list.Add(pl);
                    }
                    else
                    {
                        _columnLocations[res]--;
                    }
                    column_resp.Value.RemoveAt(0);
                }
            }
        }      
    }
}
