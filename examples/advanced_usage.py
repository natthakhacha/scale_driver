"""
Advanced example using context manager and stable readings.

This example demonstrates:
- Using context manager for automatic connection/disconnection
- Reading stable measurements only
- Reading multiple measurements
"""

from scale_driver import ScaleDriver, ScaleConfig

# Configure the scale
config = ScaleConfig(
    port="/dev/ttyUSB0",  # Change to your scale's port
    baudrate=9600,
    timeout=2.0
)

# Use context manager for automatic connection handling
with ScaleDriver(config, protocol="generic") as driver:
    print("Connected to scale using context manager\n")
    
    # Wait for and read a stable measurement
    print("Waiting for stable reading...")
    try:
        stable_reading = driver.read_stable(max_attempts=10, delay=0.5)
        print(f"Stable weight: {stable_reading.weight} {stable_reading.unit}")
    except Exception as e:
        print(f"Failed to get stable reading: {e}")
    
    # Read multiple measurements
    print("\nReading 5 measurements with 1 second interval...")
    readings = driver.read_multiple(count=5, delay=1.0)
    
    print(f"Collected {len(readings)} readings:")
    for i, reading in enumerate(readings, 1):
        print(f"  {i}. {reading.weight} {reading.unit} (stable: {reading.stable})")
    
    # Calculate average
    if readings:
        avg_weight = sum(r.weight for r in readings) / len(readings)
        print(f"\nAverage weight: {avg_weight:.2f} {readings[0].unit}")

print("\nAutomatically disconnected from scale")
