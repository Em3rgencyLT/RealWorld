namespace Domain
{
    public class Chunk<T>
    {
        private int _x;
        private int _y;
        private T _data;

        public Chunk(int x, int y, T data)
        {
            _x = x;
            _y = y;
            _data = data;
        }

        public int X => _x;

        public int Y => _y;

        public T Data => _data;
    }
}