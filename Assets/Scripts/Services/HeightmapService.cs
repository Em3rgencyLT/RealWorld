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

        public HeightmapService(int worldSize, SRTMDataService srtmDataService)
        {
            int worldSizeModifier = worldSize - worldSize % 2;
            //Unity Terrain constraint. Heightmap must be a power of 2, plus 1.
            _heightmapResolution = (int) Math.Pow(2, Parameters.HEIGHTMAP_RESOLUTION_BASE_POWER + worldSizeModifier) + 1;
            _srtmDataService = srtmDataService;
        }

        /**
         * Heightmaps are expected to be a 2D array of values from 0 to 1.
         */
        //FIXME: coordinate box should always be the same size. Once chunk size is established, this method should receive a single origin coordinate only.
        public float[,] GetHeightmapMatrix(AreaBounds<Coordinates> areaBounds)
        {
            Debug.Log(
                $"Requesting elevation data from {areaBounds.BottomPoint} to {areaBounds.TopPoint}.");
            
            var heightmap = new float[_heightmapResolution, _heightmapResolution];
            
            var bottomCoordinates = areaBounds.BottomPoint;
            var topCoordinates = areaBounds.TopPoint;
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