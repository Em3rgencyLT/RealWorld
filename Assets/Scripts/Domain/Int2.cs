namespace Domain
{
    public class Int2
    {
        private int _x;
        private int _y;

        public Int2(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X => _x;

        public int Y => _y;
    }
}