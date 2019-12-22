using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Services;

namespace Tests.Unit
{
    public class ChunkUpdateBufferServiceTests
    {
        private ChunkUpdate _createChunk1 = new ChunkUpdate(new Int2(0,1), ChunkUpdate.Type.CREATE);
        private ChunkUpdate _createChunk2 = new ChunkUpdate(new Int2(1,0), ChunkUpdate.Type.CREATE);
        private ChunkUpdate _createChunk3 = new ChunkUpdate(new Int2(0,-1), ChunkUpdate.Type.CREATE);
        private ChunkUpdate _createChunk4 = new ChunkUpdate(new Int2(-1,-1), ChunkUpdate.Type.CREATE);
        private ChunkUpdate _deleteChunk1 = new ChunkUpdate(new Int2(0,1), ChunkUpdate.Type.DELETE);

        private ChunkUpdateBufferService _service;
        
        [SetUp]
        public void SetUp()
        {
            _service = new ChunkUpdateBufferService(); 
        }
        
        [Test]
        public void CanSaveToBufferAndReturn()
        {
            _service.AddToBuffer(new List<ChunkUpdate>{_createChunk1});
            var chunkUpdate = _service.PopNext();
            
            Assert.True(chunkUpdate.IsEqualTo(_createChunk1));
            Assert.IsNull(_service.PopNext());
        }

        [Test]
        public void DecreasesBufferUsingFIFO()
        {
            _service.AddToBuffer(new List<ChunkUpdate>{_createChunk1,_createChunk2,_createChunk3,_createChunk4});
            var chunkUpdate = _service.PopNext();
            
            Assert.True(chunkUpdate.IsEqualTo(_createChunk1));
            Assert.IsNotNull(_service.PopNext());
        }
        
        [Test]
        public void IgnoresDuplicateEntries()
        {
            _service.AddToBuffer(new List<ChunkUpdate>{_createChunk1,_createChunk1,_createChunk1,_createChunk2});
            _service.AddToBuffer(new List<ChunkUpdate>{_createChunk1,_createChunk1,_createChunk2});
            var chunkUpdate1 = _service.PopNext();
            var chunkUpdate2 = _service.PopNext();
            
            Assert.True(chunkUpdate1.IsEqualTo(_createChunk1));
            Assert.True(chunkUpdate2.IsEqualTo(_createChunk2));
            Assert.IsNull(_service.PopNext());
        }
        
        [Test]
        public void CancelsOppositeEntries()
        {
            _service.AddToBuffer(new List<ChunkUpdate>{_createChunk1, _createChunk2, _deleteChunk1, _createChunk3});
            var chunkUpdate2 = _service.PopNext();
            var chunkUpdate3 = _service.PopNext();
            
            Assert.True(chunkUpdate2.IsEqualTo(_createChunk2));
            Assert.True(chunkUpdate3.IsEqualTo(_createChunk3));
            Assert.IsNull(_service.PopNext());
        }
    }
}
