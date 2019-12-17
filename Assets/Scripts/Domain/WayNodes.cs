using System.Collections.Generic;
using UnityEngine;

namespace Domain
{
    public class WayNodes
    {
        private List<Vector3> _nodes;

        public WayNodes(List<Vector3> nodes)
        {
            _nodes = nodes;
        }

        public List<Vector3> Nodes => _nodes;
    }
}