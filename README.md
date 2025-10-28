# Scale Driver - Hybrid RS232 Frame Parser

A flexible and extensible C# (.NET 8.0 / .NET Framework 4.8 compatible) application for reading and parsing weight data from RS232 scales. The system supports multiple scale formats through regex-based parsers and plugin DLLs, with automatic format detection based on confidence scoring.

## Features

- **RS232 Serial Communication**: Read data from scales via COM ports
- **Hybrid Format Support**: Parse both ASCII and binary formats using regex patterns
- **Auto-Detection**: Automatically detect the correct parser using `CanParse()` and `GetConfidenceScore()` methods
- **Plugin System**: Load custom parsers from external DLL files
- **Regex Configuration**: Create and test custom regex patterns via the Format Editor UI
- **Multiple Interfaces**: Windows Forms GUI and Console application
- **Built-in Parsers**: Includes parsers for common scale formats (Generic CSV, Simple Weight, Toledo)
- **Example Plugins**: Mettler Toledo and Ohaus scale parsers included

## Architecture

### Core Components

1. **IWeightFrameParser** - Interface for all parser implementations
2. **SerialReader** - Handles RS232 serial port communication
3. **FrameManager** - Manages multiple parsers and performs auto-detection
4. **RegexParser** - Configurable regex-based parser for custom formats
5. **PluginLoader** - Dynamically loads parser plugins from DLL files

### Projects

- **ScaleDriver.Core** - Core library with all parsing logic
- **ScaleDriver.UI** - Windows Forms application with GUI
- **ScaleDriver.Console** - Console application for testing and automation
- **ScaleDriver.Plugins** - Example plugin parsers (Mettler Toledo, Ohaus)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later (for development)
- .NET Framework 4.8 (for deployment on Windows with Visual Studio 2022)
- Visual Studio 2022 (recommended)
- Serial port (physical or virtual) for testing

### Building the Solution

```bash
# Clone the repository
git clone https://github.com/natthakhacha/scale_driver.git
cd scale_driver

# Build the solution
dotnet build ScaleDriver.sln

# Run the console application
dotnet run --project ScaleDriver.Console/ScaleDriver.Console.csproj

# Run the Windows Forms UI (Windows only)
dotnet run --project ScaleDriver.UI/ScaleDriver.UI.csproj
```

### Visual Studio 2022

1. Open `ScaleDriver.sln` in Visual Studio 2022
2. Select either `ScaleDriver.UI` or `ScaleDriver.Console` as the startup project
3. Press F5 to build and run

## Usage

### Console Application

The console application provides an interactive menu:

```
1. Connect to serial port
2. Test parser with sample data
3. Add custom regex parser
4. List parsers
5. Load plugins
0. Exit
```

#### Example Test Data

- Generic CSV: `ST,GS,+00012.50,kg`
- Simple Weight: `25.75 kg`
- Toledo Format: `ST 100.25 lb`
- Mettler Toledo: `S S    12.50 kg`
- Ohaus: `0.12 kg`

### Windows Forms GUI

The GUI application provides:

1. **Connection Panel**: Select COM port and baud rate, connect/disconnect
2. **Parsers Panel**: Add/remove parsers, load plugin DLLs
3. **Weight Display**: Shows parsed weight, unit, and stability status
4. **Status Log**: Real-time status messages
5. **Raw Data Log**: View raw serial data received

#### Format Editor

Click "Add Regex Parser" to open the Format Editor:

1. Enter a parser name and description
2. Define the regex pattern with named groups
3. Specify group names for weight, unit, and stability
4. Test the pattern with sample data
5. Save to add the parser to the system

### Creating Custom Parsers

#### Regex Parser Example

```csharp
var parser = new RegexParser(
    name: "My Scale",
    pattern: @"(?<weight>\d+\.?\d*)\s*(?<unit>kg|g)",
    weightGroup: "weight",
    unitGroup: "unit",
    stabilityGroup: null,
    description: "Custom scale format"
);

frameManager.AddParser(parser);
```

#### Plugin Parser Example

Create a new class library and implement `IWeightFrameParser`:

```csharp
using ScaleDriver.Core;

public class MyScaleParser : IWeightFrameParser
{
    public string Name => "My Scale Parser";
    public string Description => "Parser for my custom scale";

    public bool CanParse(string data)
    {
        // Check if data matches your format
        return data.StartsWith("WEIGHT:");
    }

    public int GetConfidenceScore(string data)
    {
        // Return 0-100 confidence score
        return CanParse(data) ? 80 : 0;
    }

    public WeightResult Parse(string data)
    {
        // Parse the data and return result
        var weight = /* extract weight */;
        var unit = /* extract unit */;
        return WeightResult.CreateSuccess(weight, unit);
    }
}
```

Build your plugin as a DLL and load it using the "Load Plugins" button in the UI or option 5 in the console.

## Parser Auto-Detection

The system uses a two-stage detection process:

1. **CanParse()**: Quick check if the parser can handle the data format
2. **GetConfidenceScore()**: Returns a score (0-100) indicating confidence level

The FrameManager selects the parser with the highest confidence score. This allows multiple parsers to coexist and automatically adapts to different scale formats.

### Confidence Score Guidelines

- **90-100**: Highly specific format with unique identifiers
- **70-89**: Format with specific markers (e.g., Mettler Toledo "S S")
- **50-69**: Standard format with common patterns
- **30-49**: Generic format with basic validation
- **0-29**: Low confidence or fallback parsers

## Built-in Parser Formats

### Generic CSV Parser
- **Format**: `ST,GS,+00012.50,kg`
- **Pattern**: `(?<stable>[A-Z]{2}),(?<mode>[A-Z]{2}),(?<weight>[+-]?\d+\.?\d*),(?<unit>\w+)`
- **Confidence**: Medium (50-70)

### Simple Weight Parser
- **Format**: `12.50 kg`
- **Pattern**: `(?<weight>\d+\.?\d*)\s*(?<unit>kg|g|lb|oz)`
- **Confidence**: Medium (50-70)

### Toledo Format Parser
- **Format**: `ST 100.25 lb`
- **Pattern**: `(?<stable>ST|US)\s+(?<weight>[+-]?\d+\.?\d*)\s*(?<unit>\w+)`
- **Confidence**: Medium (50-70)

### Mettler Toledo MT-SICS (Plugin)
- **Format**: `S S    12.50 kg`
- **Pattern**: Custom implementation
- **Confidence**: High (70-100)

### Ohaus Digital Scale (Plugin)
- **Format**: `0.12 kg`
- **Pattern**: Custom implementation
- **Confidence**: Low-Medium (40-60)

## Configuration

### Serial Port Settings

Common settings for industrial scales:
- **Baud Rate**: 9600 (default), 2400, 4800, 19200, 38400, 57600, 115200
- **Data Bits**: 8
- **Parity**: None
- **Stop Bits**: 1

### Testing Without Hardware

You can test the application without a physical scale using:

1. **Virtual Serial Ports**: Use tools like com0com (Windows) or socat (Linux)
2. **Test Data**: Use option 2 in the console app to test parsers with sample data
3. **Format Editor**: Test regex patterns in the UI without connecting to a port

## Troubleshooting

### No COM Ports Found
- Check if the scale is properly connected
- Verify USB drivers are installed (for USB-to-Serial adapters)
- Check Windows Device Manager for COM port assignments

### Parse Errors
- Verify the data format matches the parser pattern
- Check baud rate and serial settings
- Use Raw Data log to inspect actual data received
- Test the parser with known sample data first

### Plugin Not Loading
- Ensure the plugin DLL references ScaleDriver.Core
- Check that classes implement IWeightFrameParser
- Verify the DLL is not blocked (Windows: Properties > Unblock)
- Check console output for specific error messages

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is open source. Please check the LICENSE file for details.

## Authors

- natthakhacha

## Acknowledgments

- Built with .NET 8.0 and Windows Forms
- Uses System.IO.Ports for serial communication
- Compatible with .NET Framework 4.8 for Visual Studio 2022
