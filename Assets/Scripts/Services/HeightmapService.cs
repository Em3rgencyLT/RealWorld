using System.Collections.Generic;
using Domain;
using Domain.Tuples;
using Utility;

namespace Services
{
    public class HeightmapService
    {
        private int _heightmapResolution;
        private SRTMDataService _srtmDataService;
        private int _highestElevation;

        public HeightmapService(SRTMDataService srtmDataService, int highestElevation)
        {
            //FIXME: should be dynamic based on chunk size. Small chunks with large resolution make it look voxel-y
            _heightmapResolution = 65;
            _srtmDataService = srtmDataService;
            _highestElevation = highestElevation;
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
                    float height = (float) _srtmDataService.GetHeight(coordinates);
                    //Ignore missing or absurd values
                    if (height >= 0 && height < _highestElevation)
                    {
                        heightmap[i, j] = height / _highestElevation;
                        continue;
                    }

                    //SRTM returned shit result. Take previous value.
                    if (j > 0)
                    {
                        heightmap[i, j] = heightmap[i, j - 1];
                        continue;
                    }

                    if (i > 0)
                    {
                        heightmap[i, j] = heightmap[i - 1, j];
                    }

                    //We are the first element, there is no previous value. Sadface.
                    heightmap[i, j] = 0;
                }
            }

            return heightmap;
        }
    }
}