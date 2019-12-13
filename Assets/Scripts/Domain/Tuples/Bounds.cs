namespace Domain.Tuples
{
    /**
     * Describes a rectangle bounded by min and max points.
     * MinPoint refers to the bottom left corner of the rectangle.
     * MaxPoint refers to the top right corner of the rectangle.
     */
    public class Bounds<T>
    {
        private Bounds(T minPoint, T maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        public static Bounds<T> of(T minPoint, T maxPoint)
        {
            return new Bounds<T>(minPoint, maxPoint);
        }

        public T MinPoint { get; }

        public T MaxPoint { get; }
    }
}