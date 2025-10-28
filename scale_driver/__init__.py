"""
Scale Driver Package

A Python package for interfacing with digital weighing scales.
Supports various communication protocols including Serial/USB.
"""

__version__ = "0.1.0"
__author__ = "scale_driver"

from .driver import ScaleDriver
from .models import ScaleReading
from .exceptions import ScaleDriverError, ConnectionError, ReadError

__all__ = [
    "ScaleDriver",
    "ScaleReading",
    "ScaleDriverError",
    "ConnectionError",
    "ReadError",
]
