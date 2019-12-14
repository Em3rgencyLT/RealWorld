namespace Domain
{
    //TODO: have a proper config file
    public static class Parameters
    {
        //Overpass seems to be slower and tells you to fuck off after loading 2-3 chunks in quick succession
        //public static readonly string OSM_DATA_API_URL = @"https://overpass-api.de/api/map?bbox=";
        public static readonly string OSM_DATA_API_URL = @"https://api.openstreetmap.org/api/0.6/map?bbox=";
        public static readonly string NASA_SRTM_USERNAME = "";
        public static readonly string NASA_SRTM_PASSWORD = "";
        public static readonly int TERRAIN_LAYER = 15;
        public static readonly int CHUNK_SIZE_METERS = 256;
        public static readonly int TERRAIN_CHUNK_UNIT_RADIUS = 6;
        public static readonly int MAP_CHUNK_UNIT_RADIUS = 3;
    }
}