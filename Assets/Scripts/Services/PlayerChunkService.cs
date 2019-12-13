using Domain;
using UnityEngine;

namespace Services
{
    public class PlayerChunkService
    {
        private GameObject _playerObject;

        public PlayerChunkService(GameObject playerObject)
        {
            _playerObject = playerObject;
        }

        public Vector2 GetPlayerChunkCoordinates()
        {
            Vector3 playerPosition = _playerObject.transform.position;
            int playerChunkX = (int)Mathf.Floor(playerPosition.x / Parameters.CHUNK_SIZE_METERS);
            int playerChunkY = (int)Mathf.Floor(playerPosition.z / Parameters.CHUNK_SIZE_METERS);
            return new Vector2(playerChunkX, playerChunkY);
        }
    }
}