# Quick Start Guide

## Running the Console Application

1. Build and run the console application:
```bash
dotnet run --project ScaleDriver.Console/ScaleDriver.Console.csproj
```

2. Test parsers with sample data:
   - Select option `2` from the menu
   - Enter test data (e.g., `ST,GS,+00012.50,kg`)
   - View parsed weight output

3. View available parsers:
   - Select option `4` from the menu

## Running the Windows Forms UI

1. Build and run the UI application (Windows only):
```bash
dotnet run --project ScaleDriver.UI/ScaleDriver.UI.csproj
```

2. Connect to a scale:
   - Select COM port from dropdown
   - Choose baud rate (default: 9600)
   - Click "Connect"

3. Add custom regex parser:
   - Click "Add Regex Parser"
   - Fill in the form with parser details
   - Test the pattern with sample data
   - Click "Save"

4. Load plugin parsers:
   - Click "Load Plugins"
   - Browse to plugin DLL directory
   - Plugins will be loaded and added to parser list

## Testing Without Hardware

### Console Application
Use option 2 to test with sample data:
- `ST,GS,+00012.50,kg` - Generic CSV format
- `25.75 kg` - Simple weight format
- `ST 100.25 lb` - Toledo format
- `S S    12.50 kg` - Mettler Toledo format (requires plugin)
- `0.12 kg` - Ohaus format (requires plugin)

### Windows Forms UI
The Format Editor allows testing regex patterns without connecting to a serial port:
1. Click "Add Regex Parser"
2. Enter your pattern
3. Enter test data in the "Test Data" field
4. Click "Test Pattern" to validate

## Example Regex Patterns

### Generic CSV Format
```
Pattern: (?<stable>[A-Z]{2}),(?<mode>[A-Z]{2}),(?<weight>[+-]?\d+\.?\d*),(?<unit>\w+)
Weight Group: weight
Unit Group: unit
Stability Group: stable
Example Data: ST,GS,+00012.50,kg
```

### Simple Weight Format
```
Pattern: (?<weight>\d+\.?\d*)\s*(?<unit>kg|g|lb|oz)
Weight Group: weight
Unit Group: unit
Stability Group: (leave empty)
Example Data: 25.75 kg
```

### Toledo Format
```
Pattern: (?<stable>ST|US)\s+(?<weight>[+-]?\d+\.?\d*)\s*(?<unit>\w+)
Weight Group: weight
Unit Group: unit
Stability Group: stable
Example Data: ST 100.25 lb
```

## Creating a Plugin

1. Create a new class library project
2. Reference ScaleDriver.Core.dll
3. Implement IWeightFrameParser interface:

```csharp
using ScaleDriver.Core;

public class MyScaleParser : IWeightFrameParser
{
    public string Name => "My Scale";
    public string Description => "Custom scale parser";

    public bool CanParse(string data)
    {
        // Return true if data matches your format
        return data.StartsWith("MYFORMAT:");
    }

    public int GetConfidenceScore(string data)
    {
        // Return 0-100 confidence score
        return CanParse(data) ? 75 : 0;
    }

    public WeightResult Parse(string data)
    {
        // Parse and return result
        // Example: Extract weight from "MYFORMAT:12.5kg"
        var parts = data.Split(':');
        var value = parts[1].TrimEnd("kg".ToCharArray());
        var weight = decimal.Parse(value);
        return WeightResult.CreateSuccess(weight, "kg", true);
    }
}
```

4. Build the project as a DLL
5. Load the DLL using "Load Plugins" in the UI or option 5 in console

## Common Serial Port Settings

| Scale Type | Baud Rate | Data Bits | Parity | Stop Bits |
|-----------|-----------|-----------|--------|-----------|
| Generic   | 9600      | 8         | None   | 1         |
| Toledo    | 9600      | 7         | Even   | 1         |
| Mettler   | 9600      | 8         | None   | 1         |
| Ohaus     | 9600      | 8         | None   | 1         |

Default settings in the application: 9600 baud, 8 data bits, no parity, 1 stop bit

## Troubleshooting

**No COM ports detected:**
- Check physical connection
- Install USB drivers if using USB-to-Serial adapter
- Run as Administrator (Windows)

**Parse errors:**
- Check data format matches parser pattern
- Verify baud rate and serial settings
- View raw data log to inspect actual data received
- Test parser with known data using option 2 (console) or Format Editor (UI)

**Plugins not loading:**
- Ensure plugin DLL references correct ScaleDriver.Core version
- Check that classes implement IWeightFrameParser with public parameterless constructor
- Verify DLL is not blocked (Windows: Right-click > Properties > Unblock)
