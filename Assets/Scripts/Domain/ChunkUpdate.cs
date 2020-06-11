namespace Domain
{
    public class ChunkUpdate
    {
        private Int2 _location;
        private Type _eventType;

        public ChunkUpdate(Int2 location, Type eventType)
        {
            _location = location;
            _eventType = eventType;
        }

        public Int2 Location => _location;

        public Type EventType => _eventType;

        public enum Type
        {
            CREATE,
            LOADING,
            DELETE
        }

        public bool IsEqualTo(ChunkUpdate other)
        {
            return _location.X == other._location.X && _location.Y == other.Location.Y &&
                   _eventType == other._eventType;
        }
        
        public bool IsOppositeTo(ChunkUpdate other)
        {
            return _location.X == other._location.X && _location.Y == other.Location.Y &&
                   _eventType != other._eventType;
        }

        public bool IsLoading(ChunkUpdate other)
        {
            return _location.X == other._location.X && _location.Y == other.Location.Y &&
                   _eventType == Type.LOADING;
        }
    }
}