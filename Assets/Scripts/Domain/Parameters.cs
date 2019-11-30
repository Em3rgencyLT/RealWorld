namespace Domain
{
    public static class Parameters
    {
        /*The WORLD_SIZE_MULTIPLIER and HEIGHTMAP_RESOLUTION_BASE_POWER values correlate to world smoothness.
         Adjust if begin seeing rectangular tiling. Lowering either value will make things smoother. Lowering too
         much will decrease accuracy.*/
        //FIXME: A possible way to resolve this permanently would be to smooth the heightmap in code. This would not sacrifice height data.
        public static readonly float WORLD_SIZE_MULTIPLIER = 0.006f;
        public static readonly int HEIGHTMAP_RESOLUTION_BASE_POWER = 3;
        public static readonly int TERRAIN_LAYER = 15;
    }
}