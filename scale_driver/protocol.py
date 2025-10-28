"""
Protocol handlers for different scale communication protocols.
"""

import re
from abc import ABC, abstractmethod
from typing import Optional
from .models import ScaleReading
from .exceptions import ReadError


class ProtocolHandler(ABC):
    """Abstract base class for scale protocol handlers."""
    
    @abstractmethod
    def parse_reading(self, data: bytes) -> Optional[ScaleReading]:
        """Parse raw data into a ScaleReading object."""
        pass


class GenericProtocol(ProtocolHandler):
    """Generic protocol handler for common scale formats."""
    
    def parse_reading(self, data: bytes) -> Optional[ScaleReading]:
        """
        Parse generic scale data format.
        Expected format: "WEIGHT UNIT" e.g., "12.5 kg"
        """
        try:
            text = data.decode('ascii', errors='ignore').strip()
            if not text:
                return None
            
            # Try to extract weight and unit
            # Pattern matches: numbers (with optional decimal and sign) followed by optional unit
            pattern = r'([+-]?\d+\.?\d*)\s*([a-zA-Z]*)'
            match = re.search(pattern, text)
            
            if match:
                weight_str, unit = match.groups()
                weight = float(weight_str)
                unit = unit.lower() if unit else "kg"
                
                # Check if reading is stable (look for stability indicators)
                stable = not any(indicator in text.lower() for indicator in ['*', 'unstable', '?'])
                
                return ScaleReading(weight=weight, unit=unit, stable=stable)
            
            return None
        except (ValueError, UnicodeDecodeError) as e:
            raise ReadError(f"Failed to parse scale data: {e}")


class MettlerToledoProtocol(ProtocolHandler):
    """Protocol handler for Mettler Toledo scales (MT-SICS protocol)."""
    
    def parse_reading(self, data: bytes) -> Optional[ScaleReading]:
        """Parse Mettler Toledo MT-SICS protocol."""
        try:
            text = data.decode('ascii', errors='ignore').strip()
            
            # MT-SICS format: "S S     12.345 kg" or "S D     10.500 g"
            # First S indicates successful reading, second field (S/D) indicates stable/dynamic
            if text.startswith('S '):
                parts = text.split()
                if len(parts) >= 4:
                    stable = parts[1] == 'S'  # Second field indicates stability
                    weight = float(parts[-2])
                    unit = parts[-1].lower()
                    return ScaleReading(weight=weight, unit=unit, stable=stable)
            
            return None
        except (ValueError, UnicodeDecodeError, IndexError) as e:
            raise ReadError(f"Failed to parse Mettler Toledo data: {e}")


def get_protocol_handler(protocol_name: str = "generic") -> ProtocolHandler:
    """Get appropriate protocol handler by name."""
    protocols = {
        "generic": GenericProtocol,
        "mettler_toledo": MettlerToledoProtocol,
    }
    
    handler_class = protocols.get(protocol_name.lower())
    if handler_class is None:
        raise ValueError(f"Unknown protocol: {protocol_name}")
    
    return handler_class()
