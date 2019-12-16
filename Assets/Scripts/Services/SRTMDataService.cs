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
        private ConfigurationService _config;
        
        public SRTMDataService()
        {
            _config = new ConfigurationService(FolderPaths.ConfigFile);
            string username = _config.GetString(ConfigurationKeyString.NASA_SRTM_USERNAME);
            string password = _config.GetString(ConfigurationKeyString.NASA_SRTM_PASSWORD);
            
            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                _srtmData = new SRTMData(FolderPaths.SRTMData, new USGSSource());
            }
            else
            {
                var credentials = new NetworkCredential(username, password);
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