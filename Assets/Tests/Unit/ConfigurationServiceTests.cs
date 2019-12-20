using System.IO;
using Domain;
using NUnit.Framework;
using Services;
using Utility;

namespace Tests.Unit
{
    public class ConfigurationServiceTests
    {
        private string _location = FolderPaths.ConfigDir + FolderPaths.Slash() + "config_test.cfg";
        private ConfigurationService _service;
        
        [SetUp]
        public void SetUp()
        {
            _service = new ConfigurationService(_location); 
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_location);
        }
        
        [Test]
        public void CreatesConfigFile()
        {
            Assert.True(File.Exists(_location));
            Assert.NotNull(_service.GetInt(ConfigurationKeyInt.HIGHEST_ELEVATION_ON_EARTH));
            Assert.NotNull(_service.GetString(ConfigurationKeyString.OSM_DATA_API_URL));
        }

        [Test]
        public void DefaultConfigHasOSMRoute()
        {
            Assert.IsNotEmpty(_service.GetString(ConfigurationKeyString.OSM_DATA_API_URL));
            Assert.NotNull(_service.GetString(ConfigurationKeyString.OSM_DATA_API_URL));
        }

        [Test]
        public void DefaultConfigDoesNotHaveNASACredentials()
        {
            Assert.IsEmpty(_service.GetString(ConfigurationKeyString.NASA_SRTM_USERNAME));
            Assert.IsEmpty(_service.GetString(ConfigurationKeyString.NASA_SRTM_PASSWORD));
        }

        [Test]
        public void DefaultConfigNumbersArePositive()
        {
            Assert.Greater(_service.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS), 0);
            Assert.Greater(_service.GetInt(ConfigurationKeyInt.MAP_CHUNK_UNIT_RADIUS), 0);
            Assert.Greater(_service.GetInt(ConfigurationKeyInt.TERRAIN_CHUNK_UNIT_RADIUS), 0);
            Assert.Greater(_service.GetInt(ConfigurationKeyInt.HIGHEST_ELEVATION_ON_EARTH), 0);
        }
    }
}
