using UnityEngine;

namespace Services
{
    public class PlayerChunkService
    {
        private GameObject _playerObject;
        private int _chunkSize;

        public PlayerChunkService(GameObject playerObject, int chunkSize)
        {
            _playerObject = playerObject;
            _chunkSize = chunkSize;
        }

        public Vector2 GetPlayerChunkCoordinates()
        {
            Vector3 playerPosition = _playerObject.transform.position;
            int playerChunkX = (int)Mathf.Floor(playerPosition.x / _chunkSize);
            int playerChunkY = (int)Mathf.Floor(playerPosition.z / _chunkSize);
            return new Vector2(playerChunkX, playerChunkY);
        }
    }
}