"""
Basic example of using the scale driver.

This example demonstrates:
- Creating a scale configuration
- Connecting to a scale
- Reading a single measurement
- Disconnecting from the scale
"""

from scale_driver import ScaleDriver, ScaleConfig

# Configure the scale connection
config = ScaleConfig(
    port="/dev/ttyUSB0",  # Change to your scale's port (e.g., COM3 on Windows)
    baudrate=9600,
    timeout=2.0
)

# Create driver instance
driver = ScaleDriver(config, protocol="generic")

try:
    # Connect to scale
    print("Connecting to scale...")
    driver.connect()
    print("Connected successfully!")
    
    # Read a single measurement
    print("\nReading measurement...")
    reading = driver.read()
    
    if reading:
        print(f"Weight: {reading.weight} {reading.unit}")
        print(f"Stable: {reading.stable}")
        print(f"Timestamp: {reading.timestamp}")
    else:
        print("No data received from scale")
    
except Exception as e:
    print(f"Error: {e}")

finally:
    # Disconnect from scale
    driver.disconnect()
    print("\nDisconnected from scale")
