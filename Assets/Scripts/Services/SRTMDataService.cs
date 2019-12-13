using System.Net;
using Domain;
using SRTM;
using SRTM.Sources.NASA;
using SRTM.Sources.USGS;
using Utility;

namespace Services
{
    public class SRTMDataService
    {
        private SRTMData _srtmData;
        
        public SRTMDataService()
        {
            if (string.IsNullOrEmpty(Parameters.NASA_SRTM_USERNAME) ||
                string.IsNullOrEmpty(Parameters.NASA_SRTM_PASSWORD))
            {
                _srtmData = new SRTMData(FolderPaths.SRTMData, new USGSSource());
            }
            else
            {
                var credentials = new NetworkCredential(Parameters.NASA_SRTM_USERNAME, Parameters.NASA_SRTM_PASSWORD);
                _srtmData = new SRTMData(FolderPaths.SRTMData, new NASASource(credentials));
            }
        }

        public double GetHeight(Coordinates coordinates)
        {
            //TODO: go over all -1 values in the heightmap and average them to their neighbour values
            return _srtmData.GetElevationBilinear(coordinates.Latitude, coordinates.Longitude) ?? -1;
        }
    }
}