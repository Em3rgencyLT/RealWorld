namespace Domain
{
    public static class Parameters
    {
        public static readonly string OSM_DATA_API_URL = @"https://overpass-api.de/api/map?bbox=";

        public static readonly string NASA_SRTM_USERNAME = "";

        public static readonly string NASA_SRTM_PASSWORD = "";
        /*The WORLD_SIZE_MULTIPLIER and HEIGHTMAP_RESOLUTION_BASE_POWER values correlate to world smoothness.
         Adjust if begin seeing rectangular tiling. Lowering either value will make things smoother. Lowering too
         much will decrease accuracy.*/
        //FIXME: A possible way to resolve this permanently would be to smooth the heightmap in code. This would not sacrifice height data.
        //FIXME: Another alternative is to find a higher resolution SRTM data source
        public static readonly float WORLD_SIZE_MULTIPLIER = 0.006f;
        public static readonly int HEIGHTMAP_RESOLUTION_BASE_POWER = 3;
        public static readonly int TERRAIN_LAYER = 15;
        public static readonly int CHUNK_SIZE = 768;
        public static readonly int TERRAIN_CHUNK_DISTANCE = 2;
        public static readonly int MAP_CHUNK_DISTANCE = 5;
    }
}