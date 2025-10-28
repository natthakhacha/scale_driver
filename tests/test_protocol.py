"""
Tests for protocol handlers.
"""

import pytest
from scale_driver.protocol import GenericProtocol, MettlerToledoProtocol, get_protocol_handler
from scale_driver.exceptions import ReadError


class TestGenericProtocol:
    """Tests for GenericProtocol handler."""
    
    def test_parse_simple_reading(self):
        """Test parsing simple weight format."""
        protocol = GenericProtocol()
        data = b"12.5 kg\r\n"
        reading = protocol.parse_reading(data)
        
        assert reading is not None
        assert reading.weight == 12.5
        assert reading.unit == "kg"
        assert reading.stable is True
    
    def test_parse_reading_without_unit(self):
        """Test parsing weight without unit."""
        protocol = GenericProtocol()
        data = b"25.3\r\n"
        reading = protocol.parse_reading(data)
        
        assert reading is not None
        assert reading.weight == 25.3
        assert reading.unit == "kg"  # Default unit
    
    def test_parse_negative_weight(self):
        """Test parsing negative weight."""
        protocol = GenericProtocol()
        data = b"-5.2 lb\r\n"
        reading = protocol.parse_reading(data)
        
        assert reading is not None
        assert reading.weight == -5.2
        assert reading.unit == "lb"
    
    def test_parse_unstable_reading(self):
        """Test parsing unstable reading."""
        protocol = GenericProtocol()
        data = b"*12.5 kg\r\n"
        reading = protocol.parse_reading(data)
        
        assert reading is not None
        assert reading.stable is False
    
    def test_parse_empty_data(self):
        """Test parsing empty data."""
        protocol = GenericProtocol()
        data = b""
        reading = protocol.parse_reading(data)
        
        assert reading is None


class TestMettlerToledoProtocol:
    """Tests for MettlerToledoProtocol handler."""
    
    def test_parse_stable_reading(self):
        """Test parsing stable MT-SICS reading."""
        protocol = MettlerToledoProtocol()
        data = b"S S     12.345 kg\r\n"
        reading = protocol.parse_reading(data)
        
        assert reading is not None
        assert reading.weight == 12.345
        assert reading.unit == "kg"
        assert reading.stable is True
    
    def test_parse_unstable_reading(self):
        """Test parsing unstable MT-SICS reading."""
        protocol = MettlerToledoProtocol()
        data = b"S D     10.500 g\r\n"
        reading = protocol.parse_reading(data)
        
        assert reading is not None
        assert reading.weight == 10.500
        assert reading.unit == "g"
        assert reading.stable is False


class TestGetProtocolHandler:
    """Tests for get_protocol_handler function."""
    
    def test_get_generic_protocol(self):
        """Test getting generic protocol handler."""
        handler = get_protocol_handler("generic")
        assert isinstance(handler, GenericProtocol)
    
    def test_get_mettler_toledo_protocol(self):
        """Test getting Mettler Toledo protocol handler."""
        handler = get_protocol_handler("mettler_toledo")
        assert isinstance(handler, MettlerToledoProtocol)
    
    def test_get_unknown_protocol(self):
        """Test getting unknown protocol raises error."""
        with pytest.raises(ValueError):
            get_protocol_handler("unknown_protocol")
