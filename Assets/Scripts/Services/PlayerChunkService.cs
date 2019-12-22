using System.Collections.Generic;
using System.Linq;
using Domain;
using UnityEngine;

namespace Services
{
    public class PlayerChunkService
    {
        private GameObject _playerObject;
        private int _chunkSize;
        private int _chunkRadius;

        public PlayerChunkService(GameObject playerObject, int chunkSize, int chunkRadius)
        {
            _playerObject = playerObject;
            _chunkSize = chunkSize;
            _chunkRadius = chunkRadius;
        }

        //TODO: Not sure if this service is the correct place for this method
        public List<ChunkUpdate> GetChunkUpdates(List<Chunk> chunks)
        {
            Int2 playerChunkPosition = GetPlayerChunkCoordinates();
            int startX = playerChunkPosition.X - _chunkRadius;
            int endX = playerChunkPosition.X + _chunkRadius + 1;
            int startY = playerChunkPosition.Y - _chunkRadius;
            int endY = playerChunkPosition.Y + _chunkRadius + 1;
            
            List<ChunkUpdate> chunkUpdates = new List<ChunkUpdate>();
            chunks.Where(chunk => chunk.Location.X < startX || chunk.Location.X >= endX || chunk.Location.Y < startY || chunk.Location.Y >= endY).ToList().ForEach(
                chunk =>
                {
                    ChunkUpdate chunkDelete = new ChunkUpdate(chunk.Location, ChunkUpdate.Type.DELETE);
                    chunkUpdates.Add(chunkDelete);
                });

            for (int i = startX; i < endX; i++)
            {
                for (int j = startY; j < endY; j++)
                {
                    if (!chunks.Any(chunk => chunk.Location.X == i && chunk.Location.Y == j))
                    {
                        ChunkUpdate chunkCreate = new ChunkUpdate(new Int2(i, j), ChunkUpdate.Type.CREATE);
                        chunkUpdates.Add(chunkCreate);
                    }
                }
            }

            return chunkUpdates;
        }

        private Int2 GetPlayerChunkCoordinates()
        {
            Vector3 playerPosition = _playerObject.transform.position;
            int playerChunkX = (int)Mathf.Floor(playerPosition.x / _chunkSize);
            int playerChunkY = (int)Mathf.Floor(playerPosition.z / _chunkSize);
            return new Int2(playerChunkX, playerChunkY);
        }
    }
}