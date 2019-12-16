using Domain;
using UnityEngine;
using Utility;

namespace Services
{
    public class PlayerChunkService
    {
        private GameObject _playerObject;
        private ConfigurationService _config;

        public PlayerChunkService(GameObject playerObject)
        {
            _playerObject = playerObject;
            _config = new ConfigurationService(FolderPaths.ConfigFile);
        }

        public Vector2 GetPlayerChunkCoordinates()
        {
            Vector3 playerPosition = _playerObject.transform.position;
            int playerChunkX = (int)Mathf.Floor(playerPosition.x / _config.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS));
            int playerChunkY = (int)Mathf.Floor(playerPosition.z / _config.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS));
            return new Vector2(playerChunkX, playerChunkY);
        }
    }
}