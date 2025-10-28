"""
Data models for scale readings and configuration.
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional


@dataclass
class ScaleReading:
    """Represents a reading from a weighing scale."""
    
    weight: float
    unit: str = "kg"
    timestamp: Optional[datetime] = None
    stable: bool = True
    
    def __post_init__(self):
        """Set timestamp if not provided."""
        if self.timestamp is None:
            self.timestamp = datetime.now()
    
    def __str__(self):
        return f"{self.weight} {self.unit}"
    
    def to_dict(self):
        """Convert reading to dictionary."""
        return {
            "weight": self.weight,
            "unit": self.unit,
            "timestamp": self.timestamp.isoformat() if self.timestamp else None,
            "stable": self.stable
        }


@dataclass
class ScaleConfig:
    """Configuration for scale connection."""
    
    port: str
    baudrate: int = 9600
    bytesize: int = 8
    parity: str = "N"
    stopbits: int = 1
    timeout: float = 1.0
    
    def to_dict(self):
        """Convert config to dictionary."""
        return {
            "port": self.port,
            "baudrate": self.baudrate,
            "bytesize": self.bytesize,
            "parity": self.parity,
            "stopbits": self.stopbits,
            "timeout": self.timeout
        }
