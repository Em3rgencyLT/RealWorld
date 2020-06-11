using Domain;
using Domain.Tuples;
using Services;
using UnityEngine;

namespace Utility
{
    public class ChunkHelper
    {
        public static Bounds<Vector3> GetChunkBounds(int x, int y, int chunkSize)
        {
            int chunkXmin = x * chunkSize - chunkSize / 2;
            int chunkYmin = y * chunkSize - chunkSize / 2;
            int chunkXmax = x * chunkSize + chunkSize / 2;
            int chunkYmax = y * chunkSize + chunkSize / 2;
                    
            Vector3 minPoint = new Vector3(chunkXmin, 0f, chunkYmin);
            Vector3 maxPoint = new Vector3(chunkXmax, 0f, chunkYmax);
            
            return Bounds<Vector3>.of(minPoint, maxPoint);
        }

        public static Coordinates GetChunkCoordinates(int x, int y, int chunkSize, CoordinatePositionService coordinatePositionService)
        {
            Vector3 position = new Vector3(x * chunkSize, 0, y * chunkSize);
            return coordinatePositionService.CoordinatesFromPosition(position);
        }
    }
}