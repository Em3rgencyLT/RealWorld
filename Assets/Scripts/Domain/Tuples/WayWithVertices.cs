using System.Collections.Generic;
using UnityEngine;

namespace Domain.Tuples
{
    public class WayWithVertices
    {
        private MapElement _mapElement;
        private List<Vector3> _splineVertices;

        public WayWithVertices(MapElement mapElement, List<Vector3> splineVertices)
        {
            _mapElement = mapElement;
            _splineVertices = splineVertices;
        }

        public MapElement MapElement => _mapElement;

        public List<Vector3> SplineVertices => _splineVertices;
    }
}