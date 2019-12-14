using System.Collections.Generic;
using UnityEngine;

namespace Domain.Tuples
{
    public class WayWithVertices
    {
        private MapElement _mapElement;
        private List<Vector3> _leftVertices;
        private List<Vector3> _rightVertices;

        public WayWithVertices(MapElement mapElement, List<Vector3> leftVertices, List<Vector3> rightVertices)
        {
            _mapElement = mapElement;
            _leftVertices = leftVertices;
            _rightVertices = rightVertices;
        }

        public MapElement MapElement => _mapElement;

        public List<Vector3> LeftVertices => _leftVertices;

        public List<Vector3> RightVertices => _rightVertices;
    }
}