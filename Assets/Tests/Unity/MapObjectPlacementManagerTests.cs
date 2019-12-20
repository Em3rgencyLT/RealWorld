using System.Collections;
using Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Unity
{
    public class MapObjectPlacementManagerTests
    {
        private MapObjectPlacementManager SpawnManager()
        {
            GameObject gameObject =
                Object.Instantiate(Resources.Load<GameObject>("TestResources/Prefabs/MapObjectPlacementManager"));
            return gameObject.GetComponent<MapObjectPlacementManager>();
        }
        
        [UnityTest]
        public IEnumerator SpawnsTerrainAndObjectsWithPlayer()
        {
            MapObjectPlacementManager manager = SpawnManager();
            yield return null;
            GameObject terrainParent = GameObject.Find("Terrain");
            GameObject mapObjectsParent = GameObject.Find("Map Data");
            GameObject playerObject = GameObject.Find("shadow(Clone)");
            
            Assert.NotNull(terrainParent);
            Assert.NotNull(mapObjectsParent);
            Assert.NotNull(playerObject);
            Assert.Greater(terrainParent.transform.childCount, 0);
            Assert.Greater(mapObjectsParent.transform.childCount, 0);
            
            Object.Destroy(manager.gameObject);
            Object.Destroy(terrainParent);
            Object.Destroy(mapObjectsParent);
            Object.Destroy(playerObject);
        }
    }
}
