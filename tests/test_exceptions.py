"""
Tests for exceptions.
"""

import pytest
from scale_driver.exceptions import (
    ScaleDriverError,
    ConnectionError,
    ReadError,
    ConfigurationError,
    TimeoutError
)


class TestExceptions:
    """Tests for custom exceptions."""
    
    def test_scale_driver_error(self):
        """Test base ScaleDriverError exception."""
        with pytest.raises(ScaleDriverError):
            raise ScaleDriverError("Test error")
    
    def test_connection_error(self):
        """Test ConnectionError exception."""
        with pytest.raises(ConnectionError):
            raise ConnectionError("Connection failed")
        
        # Should also be caught as ScaleDriverError
        with pytest.raises(ScaleDriverError):
            raise ConnectionError("Connection failed")
    
    def test_read_error(self):
        """Test ReadError exception."""
        with pytest.raises(ReadError):
            raise ReadError("Read failed")
        
        # Should also be caught as ScaleDriverError
        with pytest.raises(ScaleDriverError):
            raise ReadError("Read failed")
    
    def test_configuration_error(self):
        """Test ConfigurationError exception."""
        with pytest.raises(ConfigurationError):
            raise ConfigurationError("Invalid config")
        
        # Should also be caught as ScaleDriverError
        with pytest.raises(ScaleDriverError):
            raise ConfigurationError("Invalid config")
    
    def test_timeout_error(self):
        """Test TimeoutError exception."""
        with pytest.raises(TimeoutError):
            raise TimeoutError("Operation timed out")
        
        # Should also be caught as ScaleDriverError
        with pytest.raises(ScaleDriverError):
            raise TimeoutError("Operation timed out")
