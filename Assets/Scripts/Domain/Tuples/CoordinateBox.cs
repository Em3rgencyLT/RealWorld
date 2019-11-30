namespace Domain.Tuples
{
    /**
     * Describes a box area, bounded by two real world coordinate points.
     * BottomCoordinates are the coordinates for the bottom left corner of the box.
     * TopCoordinates are the coordinates for the top right corner of the box.
     */
    public class CoordinateBox
    {
        private CoordinateBox(Coordinates bottomCoordinates, Coordinates topCoordinates)
        {
            BottomCoordinates = bottomCoordinates;
            TopCoordinates = topCoordinates;
        }

        public static CoordinateBox of(Coordinates bottomCoordinates, Coordinates topCoordinates)
        {
            return new CoordinateBox(bottomCoordinates, topCoordinates);
        }

        public Coordinates BottomCoordinates { get; }

        public Coordinates TopCoordinates { get; }
    }
}