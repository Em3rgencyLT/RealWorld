using System;
using Domain;
using NUnit.Framework;
using Services;
using UnityEngine;

namespace Tests.Unit
{
    public class CoordinatePositionServiceTests
    {
        private CoordinatePositionService _service;
        private float _precisionDegrees = 0.00000001f;
        private float _precisionMeters = 0.001f;
        
        [SetUp]
        public void SetUp()
        {
            _service = new CoordinatePositionService(Coordinates.of(0, 0)); 
        }
        
        [Test]
        public void CalculatesCorrectCoordinatesFromFirstQuadrant()
        {
            Coordinates coordinates = _service.CoordinatesFromPosition(new Vector3(1113.153f, 0, 55659.748f));
            Assert.True(Math.Abs(coordinates.Latitude - 0.5) < _precisionDegrees);
            Assert.True(Math.Abs(coordinates.Longitude - 0.01) < _precisionDegrees);
        }
        
        [Test]
        public void CalculatesCorrectCoordinatesFromSecondQuadrant()
        {
            Coordinates coordinates = _service.CoordinatesFromPosition(new Vector3(1113.153f, 0, -55659.748f));
            Assert.True(Math.Abs(coordinates.Latitude + 0.5) < _precisionDegrees);
            Assert.True(Math.Abs(coordinates.Longitude - 0.01) < _precisionDegrees);
        }
        
        [Test]
        public void CalculatesCorrectCoordinatesFromThirdQuadrant()
        {
            Coordinates coordinates = _service.CoordinatesFromPosition(new Vector3(-1113.153f, 0, -55659.748f));
            Assert.True(Math.Abs(coordinates.Latitude + 0.5) < _precisionDegrees);
            Assert.True(Math.Abs(coordinates.Longitude + 0.01) < _precisionDegrees);
        }
        
        [Test]
        public void CalculatesCorrectCoordinatesFromFourthQuadrant()
        {
            Coordinates coordinates = _service.CoordinatesFromPosition(new Vector3(-1113.153f, 0, 55659.748f));
            Assert.True(Math.Abs(coordinates.Latitude - 0.5) < _precisionDegrees);
            Assert.True(Math.Abs(coordinates.Longitude + 0.01) < _precisionDegrees);
        }

        [Test]
        public void CalculatesCorrectPositionFromFirstQuadrant()
        {
            Vector3 position = _service.PositionFromCoordinates(Coordinates.of(0.02f, 0.01f));
            Assert.True(Math.Abs(1113.194f - position.x) < _precisionMeters);
            Assert.True(Math.Abs(2226.389 - position.z) < _precisionMeters);
        }
        
        [Test]
        public void CalculatesCorrectPositionFromSecondQuadrant()
        {
            Vector3 position = _service.PositionFromCoordinates(Coordinates.of(-0.02f, 0.01f));
            Assert.True(Math.Abs(1113.194f - position.x) < _precisionMeters);
            Assert.True(Math.Abs(2226.389 + position.z) < _precisionMeters);
        }
        
        [Test]
        public void CalculatesCorrectPositionFromThirdQuadrant()
        {
            Vector3 position = _service.PositionFromCoordinates(Coordinates.of(-0.02f, -0.01f));
            Assert.True(Math.Abs(1113.194f + position.x) < _precisionMeters);
            Assert.True(Math.Abs(2226.389 + position.z) < _precisionMeters);
        }
        
        [Test]
        public void CalculatesCorrectPositionFromFourthQuadrant()
        {
            Vector3 position = _service.PositionFromCoordinates(Coordinates.of(0.02f, -0.01f));
            Assert.True(Math.Abs(1113.194f + position.x) < _precisionMeters);
            Assert.True(Math.Abs(2226.389 - position.z) < _precisionMeters);
        }
    }
}
