"""
Tests for data models.
"""

import pytest
from datetime import datetime
from scale_driver.models import ScaleReading, ScaleConfig


class TestScaleReading:
    """Tests for ScaleReading model."""
    
    def test_create_reading(self):
        """Test creating a basic scale reading."""
        reading = ScaleReading(weight=12.5, unit="kg")
        assert reading.weight == 12.5
        assert reading.unit == "kg"
        assert reading.stable is True
        assert isinstance(reading.timestamp, datetime)
    
    def test_create_reading_with_timestamp(self):
        """Test creating a reading with custom timestamp."""
        ts = datetime(2025, 1, 1, 12, 0, 0)
        reading = ScaleReading(weight=10.0, unit="kg", timestamp=ts)
        assert reading.timestamp == ts
    
    def test_reading_to_string(self):
        """Test string representation of reading."""
        reading = ScaleReading(weight=15.5, unit="lb")
        assert str(reading) == "15.5 lb"
    
    def test_reading_to_dict(self):
        """Test converting reading to dictionary."""
        reading = ScaleReading(weight=20.0, unit="kg", stable=False)
        data = reading.to_dict()
        assert data["weight"] == 20.0
        assert data["unit"] == "kg"
        assert data["stable"] is False
        assert "timestamp" in data


class TestScaleConfig:
    """Tests for ScaleConfig model."""
    
    def test_create_config(self):
        """Test creating a basic configuration."""
        config = ScaleConfig(port="/dev/ttyUSB0")
        assert config.port == "/dev/ttyUSB0"
        assert config.baudrate == 9600
        assert config.bytesize == 8
        assert config.parity == "N"
        assert config.stopbits == 1
        assert config.timeout == 1.0
    
    def test_create_config_with_custom_values(self):
        """Test creating configuration with custom values."""
        config = ScaleConfig(
            port="COM3",
            baudrate=115200,
            timeout=2.0
        )
        assert config.port == "COM3"
        assert config.baudrate == 115200
        assert config.timeout == 2.0
    
    def test_config_to_dict(self):
        """Test converting config to dictionary."""
        config = ScaleConfig(port="/dev/ttyUSB0", baudrate=19200)
        data = config.to_dict()
        assert data["port"] == "/dev/ttyUSB0"
        assert data["baudrate"] == 19200
        assert "timeout" in data
