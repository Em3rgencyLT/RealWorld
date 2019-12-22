using System.Collections.Generic;
using Domain.Tuples;
using UnityEngine;

namespace Domain
{
    public class PreparedChunk
    {
        private float[,] _heightmap;
        private Bounds<Vector3> _chunkBounds;
        private Int2 _location;
        private List<StructureWithVertices> _structureVertexData;
        private List<WayWithVertices> _wayVertexData;

        public PreparedChunk(float[,] heightmap, Bounds<Vector3> chunkBounds, Int2 location, List<StructureWithVertices> structureVertexData, List<WayWithVertices> wayVertexData)
        {
            _heightmap = heightmap;
            _chunkBounds = chunkBounds;
            _location = location;
            _structureVertexData = structureVertexData;
            _wayVertexData = wayVertexData;
        }

        public float[,] Heightmap => _heightmap;

        public Bounds<Vector3> ChunkBounds => _chunkBounds;

        public Int2 Location => _location;

        public List<StructureWithVertices> StructureVertexData => _structureVertexData;

        public List<WayWithVertices> WayVertexData => _wayVertexData;
    }
}