using System;
using Domain;
using Domain.Tuples;
using UnityEngine;

namespace Services
{
    public class HeightmapService
    {
        private int _heightmapResolution;
        private SRTMDataService _srtmDataService;

        public HeightmapService(SRTMDataService srtmDataService)
        {
            _heightmapResolution = 129;
            _srtmDataService = srtmDataService;
        }

        /**
         * Heightmaps are expected to be a 2D array of values from 0 to 1.
         */
        public float[,] GetHeightmapMatrix(Bounds<Coordinates> bounds)
        {   
            var heightmap = new float[_heightmapResolution, _heightmapResolution];
            
            var bottomCoordinates = bounds.MinPoint;
            var topCoordinates = bounds.MaxPoint;
            double stepLat = (topCoordinates.Latitude - bottomCoordinates.Latitude) / _heightmapResolution;
            double stepLong = (topCoordinates.Longitude - bottomCoordinates.Longitude) / _heightmapResolution;         

            for (int i = 0; i < _heightmapResolution; i++)
            {
                for (int j = 0; j < _heightmapResolution; j++)
                {
                    double lat = bottomCoordinates.Latitude + stepLat * i;
                    double lon = bottomCoordinates.Longitude + stepLong * j;
                    Coordinates coordinates = Coordinates.of(lat, lon);
                    float height = (float)_srtmDataService.GetHeight(coordinates);
                    
                    heightmap[i, j] = height / 8848f;
                }
            }

            return heightmap;
        }
    }
}