using Domain.Tuples;
using UnityEngine;

namespace Domain
{
    public class AsyncChunkRequest
    {
        private Bounds<Vector3> _chunkBounds;
        private Int2 _location;

        public AsyncChunkRequest(Bounds<Vector3> chunkBounds, Int2 location)
        {
            _chunkBounds = chunkBounds;
            _location = location;
        }

        public Bounds<Vector3> ChunkBounds => _chunkBounds;

        public Int2 Location => _location;
    }
}