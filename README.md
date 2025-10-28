# Scale Driver

A Python package for interfacing with digital weighing scales via serial communication (RS-232, USB).

## Features

- 🔌 Easy connection to scales via serial ports
- 📊 Support for multiple scale protocols (Generic, Mettler Toledo MT-SICS)
- 📈 Read single or multiple measurements
- ⚖️ Stable reading detection
- 🔄 Context manager support for automatic connection handling
- 🛠️ Extensible protocol system
- ✅ Comprehensive error handling

## Installation

### From source

```bash
git clone https://github.com/natthakhacha/scale_driver.git
cd scale_driver
pip install -e .
```

### Install dependencies

```bash
pip install -r requirements.txt
```

### Development dependencies

```bash
pip install -r requirements-dev.txt
```

## Quick Start

### Basic Usage

```python
from scale_driver import ScaleDriver, ScaleConfig

# Configure your scale
config = ScaleConfig(
    port="/dev/ttyUSB0",  # Change to your port (COM3 on Windows)
    baudrate=9600,
    timeout=2.0
)

# Create and connect to scale
driver = ScaleDriver(config, protocol="generic")
driver.connect()

# Read a measurement
reading = driver.read()
if reading:
    print(f"Weight: {reading.weight} {reading.unit}")

# Disconnect
driver.disconnect()
```

### Using Context Manager

```python
from scale_driver import ScaleDriver, ScaleConfig

config = ScaleConfig(port="/dev/ttyUSB0", baudrate=9600)

# Automatic connection and disconnection
with ScaleDriver(config) as driver:
    # Wait for stable reading
    reading = driver.read_stable(max_attempts=10, delay=0.5)
    print(f"Stable weight: {reading.weight} {reading.unit}")
    
    # Read multiple measurements
    readings = driver.read_multiple(count=5, delay=1.0)
    avg_weight = sum(r.weight for r in readings) / len(readings)
    print(f"Average: {avg_weight:.2f} {readings[0].unit}")
```

## Supported Protocols

### Generic Protocol
Works with most scales that output simple text format:
```
12.5 kg
25.3 lb
-5.2 g
```

### Mettler Toledo MT-SICS
For Mettler Toledo scales using the MT-SICS protocol:
```python
driver = ScaleDriver(config, protocol="mettler_toledo")
```

## Configuration

The `ScaleConfig` class accepts the following parameters:

- `port` (str, required): Serial port path (e.g., "/dev/ttyUSB0", "COM3")
- `baudrate` (int): Communication speed (default: 9600)
- `bytesize` (int): Number of data bits (default: 8)
- `parity` (str): Parity checking ("N", "E", "O") (default: "N")
- `stopbits` (int): Number of stop bits (default: 1)
- `timeout` (float): Read timeout in seconds (default: 1.0)

## API Reference

### ScaleDriver

#### Methods

- `connect()`: Establish connection to scale
- `disconnect()`: Close connection
- `is_connected()`: Check connection status
- `read(timeout=None)`: Read single measurement
- `read_stable(max_attempts=10, delay=0.5)`: Read until stable measurement
- `read_multiple(count, delay=1.0)`: Read multiple measurements
- `send_command(command)`: Send command to scale

### ScaleReading

Data model representing a scale reading:

- `weight` (float): The measured weight
- `unit` (str): Unit of measurement (kg, g, lb, etc.)
- `timestamp` (datetime): Time of reading
- `stable` (bool): Whether reading is stable

## Testing

Run the test suite:

```bash
pytest
```

Run with coverage:

```bash
pytest --cov=scale_driver --cov-report=html
```

## Examples

See the `examples/` directory for more usage examples:

- `basic_usage.py`: Simple connection and reading
- `advanced_usage.py`: Context manager and stable readings

## Error Handling

The package provides specific exceptions:

- `ScaleDriverError`: Base exception
- `ConnectionError`: Connection failures
- `ReadError`: Reading failures
- `TimeoutError`: Operation timeouts
- `ConfigurationError`: Invalid configuration

```python
from scale_driver import ScaleDriver, ConnectionError, ReadError

try:
    driver = ScaleDriver(config)
    driver.connect()
    reading = driver.read()
except ConnectionError as e:
    print(f"Connection failed: {e}")
except ReadError as e:
    print(f"Read failed: {e}")
finally:
    driver.disconnect()
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Requirements

- Python >= 3.7
- pyserial >= 3.5

## Troubleshooting

### Permission Denied on Linux
If you get permission errors accessing the serial port:
```bash
sudo usermod -a -G dialout $USER
# Log out and back in for changes to take effect
```

### Finding Your Port
**Linux/Mac:**
```bash
ls /dev/tty*
```

**Windows:**
Check Device Manager under "Ports (COM & LPT)"