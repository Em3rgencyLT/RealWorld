using System.Collections.Generic;
using System.Linq;
using Domain;

namespace Services
{
    public class ChunkUpdateBufferService
    {
        private List<ChunkUpdate> _buffer;

        public ChunkUpdateBufferService()
        {
            _buffer = new List<ChunkUpdate>();
        }

        public void AddToBuffer(List<ChunkUpdate> newEvents)
        {
            newEvents.ForEach(newEvent =>
            {
                bool isDuplicate = _buffer.Any(existing => existing.IsEqualTo(newEvent));
                bool isOpposite = _buffer.Any(existing => existing.IsOppositeTo(newEvent));
                bool isLoading = _buffer.Any(existing => existing.IsLoading(newEvent));
                if (!isDuplicate && !isOpposite && !isLoading)
                {
                    _buffer.Add(newEvent);
                }

                if (isOpposite)
                {
                    var opposite = _buffer.Find(chunkEvent => chunkEvent.IsOppositeTo(newEvent));
                    _buffer.Remove(opposite);
                }
            });
        }

        public void FinishLoading(Int2 location)
        {
            ChunkUpdate chunk = new ChunkUpdate(location, ChunkUpdate.Type.LOADING);
            ChunkUpdate loadingChunk = _buffer.FirstOrDefault(update => update.IsEqualTo(chunk));
            if (loadingChunk != null)
            {
                _buffer.Remove(loadingChunk);
            }
        }

        public ChunkUpdate PopNext()
        {
            if (_buffer.Count == 0)
            {
                return null;
            }
            
            var next = _buffer[0];
            _buffer.RemoveAt(0);
            return next;
        }
    }
}