using Domain;
using Domain.Tuples;
using SRTM;
using UnityEngine;
using Utility;

namespace Services
{
    public class SRTMDataService
    {
        private SRTMData _srtmData;
        
        public SRTMDataService()
        {
            _srtmData = new SRTMData(FolderPaths.SRTMData);
        }

        public double GetHeight(Coordinates coordinates)
        {
            //TODO: go over all -1 values in the heightmap and average them to their neighbour values
            return _srtmData.GetElevation(coordinates.Latitude, coordinates.Longitude) ?? -1;
        }
    }
}