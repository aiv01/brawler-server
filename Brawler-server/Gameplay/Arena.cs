using System;
using System.Collections.Generic;
using BrawlerServer.Utilities;

namespace BrawlerServer.Gameplay
{
    public class Arena
    {
        public List<Position> spawnPoints { get; private set; }
        public int weaponCount { get; private set; }

        public Arena()
        {
            spawnPoints = new List<Position>();
        }

        public void AddSpawnPoint(float x, float y, float z)
        {
            spawnPoints.Add(new Position(x, y, z));
        }

        public void AddSpawnPoint(Position position)
        {
            AddSpawnPoint(position.X, position.Y, position.Z);
        }

        public void SetWeaponCount(int amount)
        {
            weaponCount = amount;
        }
    }
}
