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
        private string _username;
        private string _password;
        
        public SRTMDataService(string username, string password)
        {
            _username = username;
            _password = password;
            
            if (string.IsNullOrEmpty(_username) ||
                string.IsNullOrEmpty(_password))
            {
                _srtmData = new SRTMData(FolderPaths.SRTMData, new USGSSource());
            }
            else
            {
                var credentials = new NetworkCredential(_username, _password);
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