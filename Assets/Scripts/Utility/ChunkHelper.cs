using Domain;
using Domain.Tuples;
using UnityEngine;

namespace Utility
{
    public class ChunkHelper
    {
        public static Bounds<Vector3> GetChunkBounds(int x, int y)
        {
            int chunkXmin = x * Parameters.CHUNK_SIZE_METERS - Parameters.CHUNK_SIZE_METERS / 2;
            int chunkYmin = y * Parameters.CHUNK_SIZE_METERS - Parameters.CHUNK_SIZE_METERS / 2;
            int chunkXmax = x * Parameters.CHUNK_SIZE_METERS + Parameters.CHUNK_SIZE_METERS / 2;
            int chunkYmax = y * Parameters.CHUNK_SIZE_METERS + Parameters.CHUNK_SIZE_METERS / 2;
                    
            Vector3 minPoint = new Vector3(chunkXmin, 0f, chunkYmin);
            Vector3 maxPoint = new Vector3(chunkXmax, 0f, chunkYmax);
            
            return Bounds<Vector3>.of(minPoint, maxPoint);
        }
    }
}