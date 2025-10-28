"""
Custom exceptions for scale driver operations.
"""


class ScaleDriverError(Exception):
    """Base exception for all scale driver errors."""
    pass


class ConnectionError(ScaleDriverError):
    """Raised when connection to scale fails."""
    pass


class ReadError(ScaleDriverError):
    """Raised when reading from scale fails."""
    pass


class ConfigurationError(ScaleDriverError):
    """Raised when configuration is invalid."""
    pass


class TimeoutError(ScaleDriverError):
    """Raised when operation times out."""
    pass
