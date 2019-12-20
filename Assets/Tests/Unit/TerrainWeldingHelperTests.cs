using System;
using NUnit.Framework;
using UnityEngine;
using Utility;
using Object = UnityEngine.Object;

namespace Tests.Unit
{
    public class TerrainWeldingHelperTests
    {
        private float[,] heightmap = new float[33, 33];
        private int lastIndex = 32;

        private Terrain _terrain1;
        private Terrain _terrain2;

        [SetUp]
        public void SetUp()
        {
            float k = 0.001f;
            for (int i = 0; i < 33; i++)
            {
                for (int j = 0; j < 33; j++)
                {
                    if (i == 0 || j == 0 || i == lastIndex || j == lastIndex)
                    {
                        heightmap[j, i] = k;
                        k += 0.001f;
                        continue;
                    }

                    heightmap[j, i] = 0;
                }
            }

            var data = new TerrainData();
            data.SetHeights(0, 0, heightmap);
            GameObject terrainObject = new GameObject("Terrain1");
            _terrain1 = terrainObject.AddComponent<Terrain>();
            _terrain1.terrainData = data;
            GameObject terrainObject2 = new GameObject("Terrain2");
            _terrain2 = terrainObject2.AddComponent<Terrain>();
            _terrain2.terrainData = data;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_terrain1.gameObject);
            Object.DestroyImmediate(_terrain2.gameObject);
        }

        [Test]
        public void WeldsTheNorthernEdge()
        {
            _terrain1 = TerrainWeldingHelper.Weld(_terrain1, _terrain2, TerrainWeldingHelper.Direction.NORTH);
            var newHeightmap = _terrain1.terrainData.GetHeights(0, 0, heightmap.GetLength(0), heightmap.GetLength(1));

            Assert.IsTrue(Math.Abs((double) heightmap[0, 0] - newHeightmap[lastIndex, 0]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[0, 1] - newHeightmap[lastIndex, 1]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[0, 2] - newHeightmap[lastIndex, 2]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[0, 3] - newHeightmap[lastIndex, 3]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[0, lastIndex - 3] - newHeightmap[lastIndex, lastIndex - 3]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[0, lastIndex - 2] - newHeightmap[lastIndex, lastIndex - 2]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[0, lastIndex - 1] - newHeightmap[lastIndex, lastIndex - 1]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[0, lastIndex] - newHeightmap[lastIndex, lastIndex]) < 0.01);
        }

        [Test]
        public void WeldsTheSouthernEdge()
        {
            _terrain1 = TerrainWeldingHelper.Weld(_terrain1, _terrain2, TerrainWeldingHelper.Direction.SOUTH);
            var newHeightmap = _terrain1.terrainData.GetHeights(0, 0, heightmap.GetLength(0), heightmap.GetLength(1));

            Assert.IsTrue(Math.Abs((double) newHeightmap[0, 0] - heightmap[lastIndex, 0]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[0, 1] - heightmap[lastIndex, 1]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[0, 2] - heightmap[lastIndex, 2]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[0, 3] - heightmap[lastIndex, 3]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[0, lastIndex - 3] - heightmap[lastIndex, lastIndex - 3]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[0, lastIndex - 2] - heightmap[lastIndex, lastIndex - 2]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[0, lastIndex - 1] - heightmap[lastIndex, lastIndex - 1]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[0, lastIndex] - heightmap[lastIndex, lastIndex]) < 0.01);
        }
        
        [Test]
        public void WeldsTheEasternEdge()
        {
            _terrain1 = TerrainWeldingHelper.Weld(_terrain1, _terrain2, TerrainWeldingHelper.Direction.EAST);
            var newHeightmap = _terrain1.terrainData.GetHeights(0, 0, heightmap.GetLength(0), heightmap.GetLength(1));

            Assert.IsTrue(Math.Abs((double) newHeightmap[0, lastIndex] - heightmap[0, lastIndex]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[1, lastIndex] - heightmap[1, lastIndex]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[2, lastIndex] - heightmap[2, lastIndex]) < 0.01);
            Assert.IsTrue(Math.Abs((double) newHeightmap[3, lastIndex] - heightmap[3, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[lastIndex - 3, lastIndex] - heightmap[lastIndex - 3, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[lastIndex - 2, lastIndex] - heightmap[lastIndex - 2, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[lastIndex - 1, lastIndex] - heightmap[lastIndex - 1, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) newHeightmap[lastIndex, lastIndex] - heightmap[lastIndex, lastIndex]) < 0.01);
        }

        [Test]
        public void WeldsTheWesternEdge()
        {
            _terrain1 = TerrainWeldingHelper.Weld(_terrain1, _terrain2, TerrainWeldingHelper.Direction.WEST);
            var newHeightmap = _terrain1.terrainData.GetHeights(0, 0, heightmap.GetLength(0), heightmap.GetLength(1));

            Assert.IsTrue(Math.Abs((double) heightmap[0, lastIndex] - newHeightmap[0, lastIndex]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[1, lastIndex] - newHeightmap[1, lastIndex]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[2, lastIndex] - newHeightmap[2, lastIndex]) < 0.01);
            Assert.IsTrue(Math.Abs((double) heightmap[3, lastIndex] - newHeightmap[3, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[lastIndex - 3, lastIndex] - newHeightmap[lastIndex - 3, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[lastIndex - 2, lastIndex] - newHeightmap[lastIndex - 2, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[lastIndex - 1, lastIndex] - newHeightmap[lastIndex - 1, lastIndex]) < 0.01);
            Assert.IsTrue(
                Math.Abs((double) heightmap[lastIndex, lastIndex] - newHeightmap[lastIndex, lastIndex]) < 0.01);
        }
    }
}