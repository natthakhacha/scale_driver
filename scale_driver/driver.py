"""
Main scale driver implementation.
"""

import time
from typing import Optional, List
import serial
from .models import ScaleReading, ScaleConfig
from .protocol import ProtocolHandler, get_protocol_handler
from .exceptions import ConnectionError, ReadError, TimeoutError


class ScaleDriver:
    """
    Main driver class for interfacing with weighing scales.
    
    Supports serial communication with various scale protocols.
    """
    
    def __init__(self, config: ScaleConfig, protocol: str = "generic"):
        """
        Initialize scale driver.
        
        Args:
            config: Scale configuration object
            protocol: Protocol name (default: "generic")
        """
        self.config = config
        self.protocol_handler: ProtocolHandler = get_protocol_handler(protocol)
        self.connection: Optional[serial.Serial] = None
        self._is_connected = False
    
    def connect(self) -> bool:
        """
        Establish connection to the scale.
        
        Returns:
            True if connection successful
            
        Raises:
            ConnectionError: If connection fails
        """
        try:
            self.connection = serial.Serial(
                port=self.config.port,
                baudrate=self.config.baudrate,
                bytesize=self.config.bytesize,
                parity=self.config.parity,
                stopbits=self.config.stopbits,
                timeout=self.config.timeout
            )
            self._is_connected = True
            return True
        except serial.SerialException as e:
            raise ConnectionError(f"Failed to connect to scale: {e}")
    
    def disconnect(self):
        """Close connection to the scale."""
        if self.connection and self.connection.is_open:
            self.connection.close()
        self._is_connected = False
    
    def is_connected(self) -> bool:
        """Check if scale is connected."""
        return self._is_connected and self.connection and self.connection.is_open
    
    def read(self, timeout: Optional[float] = None) -> Optional[ScaleReading]:
        """
        Read a single measurement from the scale.
        
        Args:
            timeout: Optional timeout in seconds (overrides config timeout)
            
        Returns:
            ScaleReading object or None if no data available
            
        Raises:
            ConnectionError: If not connected
            ReadError: If reading fails
            TimeoutError: If operation times out
        """
        if not self.is_connected():
            raise ConnectionError("Not connected to scale")
        
        try:
            if timeout:
                original_timeout = self.connection.timeout
                self.connection.timeout = timeout
            
            # Read until newline or timeout
            data = self.connection.readline()
            
            if timeout:
                self.connection.timeout = original_timeout
            
            if not data:
                return None
            
            return self.protocol_handler.parse_reading(data)
            
        except serial.SerialException as e:
            raise ReadError(f"Failed to read from scale: {e}")
    
    def read_stable(self, max_attempts: int = 10, delay: float = 0.5) -> ScaleReading:
        """
        Read from scale until a stable reading is obtained.
        
        Args:
            max_attempts: Maximum number of read attempts
            delay: Delay between attempts in seconds
            
        Returns:
            Stable ScaleReading object
            
        Raises:
            TimeoutError: If stable reading not obtained within max_attempts
            ConnectionError: If not connected
            ReadError: If reading fails
        """
        for attempt in range(max_attempts):
            reading = self.read()
            if reading and reading.stable:
                return reading
            time.sleep(delay)
        
        raise TimeoutError(f"Failed to get stable reading after {max_attempts} attempts")
    
    def read_multiple(self, count: int, delay: float = 1.0) -> List[ScaleReading]:
        """
        Read multiple measurements from the scale.
        
        Args:
            count: Number of readings to take
            delay: Delay between readings in seconds
            
        Returns:
            List of ScaleReading objects
            
        Raises:
            ConnectionError: If not connected
            ReadError: If reading fails
        """
        readings = []
        for _ in range(count):
            reading = self.read()
            if reading:
                readings.append(reading)
            if delay > 0:
                time.sleep(delay)
        return readings
    
    def send_command(self, command: str) -> bool:
        """
        Send a command to the scale.
        
        Args:
            command: Command string to send
            
        Returns:
            True if command sent successfully
            
        Raises:
            ConnectionError: If not connected
        """
        if not self.is_connected():
            raise ConnectionError("Not connected to scale")
        
        try:
            self.connection.write(command.encode('ascii'))
            return True
        except serial.SerialException as e:
            raise ConnectionError(f"Failed to send command: {e}")
    
    def __enter__(self):
        """Context manager entry."""
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit."""
        self.disconnect()
