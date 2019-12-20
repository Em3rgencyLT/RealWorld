using UnityEngine;

namespace Utility
{
    public class TerrainWeldingHelper
    {
        public enum Direction
        {
            NORTH,
            EAST,
            SOUTH,
            WEST
        }

        public static Terrain Weld(Terrain main, Terrain other, Direction direction)
        {
            int size = main.terrainData.heightmapResolution;
            int start = 0;
            int end = size - 1;

            switch (direction)
            {
                //North = down, South = up;
                case Direction.NORTH:
                    return WeldEdge(main, other, end, end, start, end, start, start);
                case Direction.SOUTH:
                    return WeldEdge(main, other, start, start, start, end, end, start);
                case Direction.EAST:
                    return WeldEdge(main, other, end, start, start, start, start, start);
                case Direction.WEST:
                    return WeldEdge(main, other, start, end, start, start, start, end);
                default:
                    return main;
            }
        }

        private static Terrain WeldEdge(Terrain main, Terrain other, int iStartMain, int iEndMain, int jStartMain,
            int jEndMain, int iStartOther, int jStartOther)
        {
            int size = main.terrainData.heightmapResolution;
            var mainHeightmap = main.terrainData.GetHeights(0, 0, size, size);
            var otherHeightmap = other.terrainData.GetHeights(0, 0, size, size);
            for (int i = iStartMain, i1 = iStartOther; i < iEndMain + 1; i++, i1++)
            {
                for (int j = jStartMain, j1 = jStartOther; j < jEndMain + 1; j++, j1++)
                {
                    mainHeightmap[i, j] = otherHeightmap[i1, j1];
                }
            }
            
            main.terrainData.SetHeights(0,0, mainHeightmap);
            return main;
        }
    }
}