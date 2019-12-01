namespace Domain.Tuples
{
    /**
     * Describes an area, bounded by two points.
     * BottomPoint refers to the bottom left corner of the box.
     * TopPoint refers to the top right corner of the box.
     */
    public class AreaBounds<T>
    {
        private AreaBounds(T bottomPoint, T topPoint)
        {
            BottomPoint = bottomPoint;
            TopPoint = topPoint;
        }

        public static AreaBounds<T> of(T bottomPoint, T topPoint)
        {
            return new AreaBounds<T>(bottomPoint, topPoint);
        }

        public T BottomPoint { get; }

        public T TopPoint { get; }
    }
}