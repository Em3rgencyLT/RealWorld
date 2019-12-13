using System.Collections.Generic;
using UnityEngine;

namespace Domain.Tuples
{
    public class StructureWithVertices
    {
        private MapElement _mapElement;
        private List<Vector3> _vertices;

        public StructureWithVertices(MapElement mapElement, List<Vector3> vertices)
        {
            _mapElement = mapElement;
            _vertices = vertices;
        }

        public MapElement MapElement => _mapElement;

        public List<Vector3> Vertices => _vertices;
    }
}