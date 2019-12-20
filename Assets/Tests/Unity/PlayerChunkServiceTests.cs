using System.Collections;
using NUnit.Framework;
using Services;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Unity
{
    public class PlayerChunkServiceTests
    {
        private GameObject _player;
        
        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Player");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_player);
        }
        
        [UnityTest]
        public IEnumerable ShouldFindPlayerChunkWithSmallChunks()
        {
            PlayerChunkService service = new PlayerChunkService(_player, 10);
            _player.transform.position = new Vector3(65, -20, -114);
            Vector2 chunkCoordinates = service.GetPlayerChunkCoordinates();

            yield return null;
            
            Assert.AreSame(6f, chunkCoordinates.x);
            Assert.AreSame(-11f, chunkCoordinates.y);
        }
        
        [UnityTest]
        public IEnumerable ShouldFindPlayerChunkWithLargeChunks()
        {
            PlayerChunkService service = new PlayerChunkService(_player, 1000);
            _player.transform.position = new Vector3(500, 0, 1499);
            Vector2 chunkCoordinates = service.GetPlayerChunkCoordinates();
            
            yield return null;
            
            Assert.AreSame(0f, chunkCoordinates.x);
            Assert.AreSame(1f, chunkCoordinates.y);
        }
    }
}
