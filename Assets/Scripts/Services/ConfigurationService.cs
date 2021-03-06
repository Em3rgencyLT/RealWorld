using System;
using System.IO;
using Domain;
using SharpConfig;

namespace Services
{
    public class ConfigurationService
    {
        private static readonly double CURRENT_VERSION = 0.03;
        private string _filepath;
        private Configuration _configuration;

        public ConfigurationService(string filepath)
        {
            _filepath = filepath;
            _configuration = SetupConfiguration();
        }

        public int GetInt(ConfigurationKeyInt key)
        {
            foreach (var section in _configuration)
            {
                if (section.Contains(key.ToString()))
                {
                    return section[key.ToString()].IntValue;
                }
            }
            
            throw new Exception($"Could not find integer setting {key} in {_filepath}!");
        }
        
        public string GetString(ConfigurationKeyString key)
        {
            foreach (var section in _configuration)
            {
                if (section.Contains(key.ToString()))
                {
                    return section[key.ToString()].StringValue;
                }
            }
            
            throw new Exception($"Could not find string setting {key} in {_filepath}!");
        }

        private Configuration SetupConfiguration()
        {
            if (!File.Exists(_filepath))
            {
                var config = GenerateDefaultConfig();
                config.SaveToFile(_filepath);
                return config;
            }

            var existingConfig = Configuration.LoadFromFile(_filepath);
            if (IsConfigurationCurrent(existingConfig))
            {
                return existingConfig;
            }
            
            File.Delete(_filepath);
            var newConfig = GenerateDefaultConfig();
            newConfig.SaveToFile(_filepath);
            return newConfig;
        }

        private bool IsConfigurationCurrent(Configuration configuration)
        {
            return Math.Abs(configuration["Internal"]["CONFIGURATION_VERSION"].DoubleValue - CURRENT_VERSION) < 0.001;
        }
        
        private Configuration GenerateDefaultConfig()
        {
            var configuration = new Configuration();
            configuration["Data Sources"][ConfigurationKeyString.OSM_DATA_API_URL.ToString()].StringValue = @"https://api.openstreetmap.org/api/0.6/map?bbox=";
            configuration["Data Sources"][ConfigurationKeyString.NASA_SRTM_USERNAME.ToString()].PreComment = "https://urs.earthdata.nasa.gov/users/new/";
            configuration["Data Sources"][ConfigurationKeyString.NASA_SRTM_USERNAME.ToString()].StringValue = "";
            configuration["Data Sources"][ConfigurationKeyString.NASA_SRTM_PASSWORD.ToString()].StringValue = "";
            configuration["Measurements"][ConfigurationKeyInt.CHUNK_SIZE_METERS.ToString()].IntValue = 256;
            configuration["Measurements"][ConfigurationKeyInt.CHUNK_UNIT_RADIUS.ToString()].IntValue = 4;
            configuration["Measurements"][ConfigurationKeyInt.HIGHEST_ELEVATION_ON_EARTH.ToString()].IntValue = 8848;
            configuration["Internal"]["CONFIGURATION_VERSION"].DoubleValue = CURRENT_VERSION;
            configuration["Internal"][ConfigurationKeyInt.TERRAIN_LAYER.ToString()].IntValue = 15;
            return configuration;
        }
    }
}