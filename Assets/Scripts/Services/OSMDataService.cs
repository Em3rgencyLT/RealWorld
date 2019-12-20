using System;
using System.IO;
using System.Xml.Linq;
using Domain;
using Domain.Tuples;
using UnityEngine;
using Utility;

namespace Services
{
    public class OSMDataService
    {
        private static readonly string FILE_EXTENSION = ".osm";
        private string _APIUrl;

        public OSMDataService(string OSMDataAPIUrl)
        {
            _APIUrl = OSMDataAPIUrl;
        }
        
        public XElement GetDataForArea(Bounds<Coordinates> bounds)
        {
            string filename =
                $"{bounds.MinPoint.Longitude},{bounds.MinPoint.Latitude},{bounds.MaxPoint.Longitude},{bounds.MaxPoint.Latitude}";

            if (!DownloadDataToFile(filename))
            {
                throw new IOException("Could not download OSM data.");
            }
            
            return ReadFile(filename);
        }

        private bool DownloadDataToFile(string commaSeparatedBounds)
        {
            string filename = Path.Combine(FolderPaths.OSMData, commaSeparatedBounds + FILE_EXTENSION);
            if (File.Exists(filename))
            {
                return true;
            }

            try
            {
                string url = _APIUrl + commaSeparatedBounds;
                string rawXml = HttpRequest.Get(url);
                File.WriteAllText(filename, rawXml);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        private XElement ReadFile(string commaSeparatedBounds)
        {
            string file = Path.Combine(FolderPaths.OSMData, commaSeparatedBounds + FILE_EXTENSION);
            if (!File.Exists(file))
            {
                throw new IOException($"Could not find OSM file {file}");
            }
            
            var contents = File.ReadAllText(file);
            return XElement.Parse(contents);
        }
    }
}