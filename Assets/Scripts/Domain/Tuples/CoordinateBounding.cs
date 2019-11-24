namespace Domain.Tuples
{
    public class CoordinateBounding
    {
        private CoordinateBounding(Coordinates bottomCoordinates, Coordinates topCoordinates)
        {
            BottomCoordinates = bottomCoordinates;
            TopCoordinates = topCoordinates;
        }

        public static CoordinateBounding of(Coordinates bottomCoordinates, Coordinates topCoordinates)
        {
            return new CoordinateBounding(bottomCoordinates, topCoordinates);
        }

        public Coordinates BottomCoordinates { get; }

        public Coordinates TopCoordinates { get; }
    }
}