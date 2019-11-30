using Domain.Tuples;
using SRTM;
using UnityEngine;
using Utility;

namespace Services
{
    public class ElevationService
    {
        private int _heightmapResolution;

        public ElevationService(int heightmapResolution)
        {
            _heightmapResolution = heightmapResolution;
        }

        //FIXME: coordinate box should always be the same size. Once chunk size is established, this method should receive a single origin coordinate only.
        public Vector3[,] GetElevationMap(CoordinateBox coordinateBox)
        {
            Debug.Log(
                $"Requesting elevation data from {coordinateBox.BottomCoordinates} to {coordinateBox.TopCoordinates}.");
            return ReadElevationData(coordinateBox);
        }

        private Vector3[,] ReadElevationData(CoordinateBox coordinateBox)
        {
            var bottomCoordinates = coordinateBox.BottomCoordinates;
            var topCoordinates = coordinateBox.TopCoordinates;

            double stepLat = (topCoordinates.Latitude - bottomCoordinates.Latitude) / _heightmapResolution;
            double stepLong = (topCoordinates.Longitude - bottomCoordinates.Longitude) / _heightmapResolution;

            Vector3[,] data = new Vector3[_heightmapResolution, _heightmapResolution];
            var srtmData = new SRTMData(FolderPaths.SRTMData);

            for (int i = 0; i < _heightmapResolution; i++)
            {
                for (int j = 0; j < _heightmapResolution; j++)
                {
                    double lat = bottomCoordinates.Latitude + stepLat * i;
                    double lon = bottomCoordinates.Longitude + stepLong * j;
                    double? elevation = srtmData.GetElevation(lat, lon);
                    double height = 0;

                    if (elevation.HasValue)
                    {
                        height = elevation.Value;
                    }

                    Vector3 basePosition = CoordinateMath.CoordinatesToWorldPosition(lat,lon);
                    data[i, j] = new Vector3(basePosition.x, (float) height, basePosition.z);
                }
            }

            return data;
        }
    }
}