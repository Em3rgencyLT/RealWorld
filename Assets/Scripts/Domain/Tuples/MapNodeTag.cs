namespace Domain.Tuples
{
    public class MapNodeTag
    {
        private MapNodeKey.KeyType _keyType;
        private string _value;

        public MapNodeTag(MapNodeKey.KeyType keyType, string value)
        {
            _keyType = keyType;
            _value = value;
        }

        public MapNodeKey.KeyType KeyType => _keyType;

        public string Value => _value;
    }
}