# System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Scale Driver System                          │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐         ┌──────────────────────────┐
│   Windows Forms GUI      │         │   Console Application    │
│   (ScaleDriver.UI)       │         │  (ScaleDriver.Console)   │
├──────────────────────────┤         ├──────────────────────────┤
│ • MainForm               │         │ • Interactive Menu       │
│ • FormatEditorForm       │         │ • Parser Testing         │
│ • Connection Management  │         │ • Plugin Management      │
│ • Real-time Display      │         │ • Automation Support     │
└────────────┬─────────────┘         └───────────┬──────────────┘
             │                                   │
             └─────────────┬─────────────────────┘
                           │
                           ▼
            ┌──────────────────────────────────────┐
            │     Core Library (ScaleDriver.Core)  │
            ├──────────────────────────────────────┤
            │                                      │
            │  ┌───────────────────────────────┐  │
            │  │    IWeightFrameParser         │  │
            │  │    (Interface)                │  │
            │  │  • CanParse()                 │  │
            │  │  • GetConfidenceScore()       │  │
            │  │  • Parse()                    │  │
            │  └───────────┬───────────────────┘  │
            │              │                       │
            │              │ implements            │
            │              ▼                       │
            │  ┌───────────────────────────────┐  │
            │  │      RegexParser              │  │
            │  │  • Configurable Patterns      │  │
            │  │  • Named Capture Groups       │  │
            │  │  • Confidence Scoring         │  │
            │  └───────────────────────────────┘  │
            │                                      │
            │  ┌───────────────────────────────┐  │
            │  │      FrameManager             │  │
            │  │  • Multi-parser Support       │  │
            │  │  • Auto-detection             │  │
            │  │  • Confidence-based Selection │  │
            │  └───────────────────────────────┘  │
            │                                      │
            │  ┌───────────────────────────────┐  │
            │  │      SerialReader             │  │
            │  │  • RS232 Communication        │  │
            │  │  • Event-driven Reception     │  │
            │  │  • Buffer Management          │  │
            │  └───────────────────────────────┘  │
            │                                      │
            │  ┌───────────────────────────────┐  │
            │  │      PluginLoader             │  │
            │  │  • Dynamic DLL Loading        │  │
            │  │  • Assembly Scanning          │  │
            │  │  • Parser Instantiation       │  │
            │  └───────────────────────────────┘  │
            │                                      │
            └──────────────────────────────────────┘
                           │
                           │ loads
                           ▼
            ┌──────────────────────────────────────┐
            │  Plugin Parsers (ScaleDriver.Plugins)│
            ├──────────────────────────────────────┤
            │ • MettlerToledoParser                │
            │ • OhausParser                        │
            │ • (Custom User Plugins)              │
            └──────────────────────────────────────┘
                           │
                           │
                           ▼
            ┌──────────────────────────────────────┐
            │         Physical Hardware            │
            ├──────────────────────────────────────┤
            │  RS232 Serial Port (COM1, COM2, etc.)│
            │           ▼                          │
            │  Industrial Scales                   │
            │  • Toledo                            │
            │  • Mettler Toledo                    │
            │  • Ohaus                             │
            │  • Generic Scales                    │
            └──────────────────────────────────────┘
```

## Data Flow

```
1. Serial Port Data Reception
   ┌──────────────┐
   │ Scale Device │
   └──────┬───────┘
          │ RS232 Frame
          ▼
   ┌──────────────┐
   │ SerialReader │
   └──────┬───────┘
          │ Raw String
          ▼

2. Frame Management & Parser Selection
   ┌──────────────┐
   │ FrameManager │
   └──────┬───────┘
          │
          ├─► Parser 1: CanParse() → true/false
          │             GetConfidenceScore() → 0-100
          │
          ├─► Parser 2: CanParse() → true/false
          │             GetConfidenceScore() → 0-100
          │
          └─► Parser N: CanParse() → true/false
                        GetConfidenceScore() → 0-100
          │
          ▼ (Select highest confidence)

3. Data Parsing
   ┌──────────────────┐
   │ Selected Parser  │
   │  (RegexParser or │
   │   Plugin Parser) │
   └──────┬───────────┘
          │ Parse()
          ▼
   ┌──────────────────┐
   │  WeightResult    │
   │  • Weight: 12.50 │
   │  • Unit: kg      │
   │  • Stable: true  │
   └──────┬───────────┘
          │
          ▼

4. Display
   ┌──────────────────┐
   │  UI / Console    │
   │  • Show Weight   │
   │  • Show Unit     │
   │  • Show Status   │
   └──────────────────┘
```

## Parser Confidence Scoring

```
High Confidence (90-100)
│  Highly specific format with unique identifiers
│  Example: Mettler Toledo with "S S" prefix
│
├─ Medium-High (70-89)
│  Format with specific markers
│  Example: Toledo with "ST" or "US" prefix
│
├─ Medium (50-69)
│  Standard format with common patterns
│  Example: Generic CSV with multiple fields
│
├─ Low-Medium (30-49)
│  Generic format with basic validation
│  Example: Simple "weight unit" format
│
└─ Low (0-29)
   Fallback parsers or no match
```

## Plugin Architecture

```
┌─────────────────────────────────────────────┐
│          User's Custom Plugin DLL           │
├─────────────────────────────────────────────┤
│                                             │
│  public class MyScaleParser                 │
│      : IWeightFrameParser                   │
│  {                                          │
│      public string Name { get; }            │
│      public string Description { get; }     │
│                                             │
│      public bool CanParse(string data)      │
│      {                                      │
│          // Custom logic                    │
│      }                                      │
│                                             │
│      public int GetConfidenceScore(...)     │
│      {                                      │
│          // Return 0-100                    │
│      }                                      │
│                                             │
│      public WeightResult Parse(...)         │
│      {                                      │
│          // Parse and return result         │
│      }                                      │
│  }                                          │
│                                             │
└─────────────────────────────────────────────┘
                    │
                    │ References
                    ▼
┌─────────────────────────────────────────────┐
│         ScaleDriver.Core.dll                │
│         (Interface Definition)              │
└─────────────────────────────────────────────┘
                    │
                    │ Loaded by
                    ▼
┌─────────────────────────────────────────────┐
│            PluginLoader                     │
│  • Scans directory for *.dll                │
│  • Loads assemblies                         │
│  • Finds IWeightFrameParser implementations │
│  • Creates instances                        │
│  • Adds to FrameManager                     │
└─────────────────────────────────────────────┘
```

## Configuration Flow

```
Format Editor UI
     │
     ├─► Enter Parser Name
     ├─► Enter Regex Pattern
     ├─► Define Capture Groups
     │   • weight
     │   • unit
     │   • stable
     ├─► Enter Test Data
     │
     └─► Test Pattern
           │
           ├─► Validate Regex
           ├─► Test Match
           ├─► Extract Groups
           ├─► Show Results
           │
           └─► Save
                 │
                 ▼
           Create RegexParser Instance
                 │
                 ▼
           Add to FrameManager
```

## Project Structure

```
ScaleDriver.sln
│
├── ScaleDriver.Core/          (Class Library)
│   ├── IWeightFrameParser.cs
│   ├── WeightResult.cs
│   ├── SerialReader.cs
│   ├── FrameManager.cs
│   ├── RegexParser.cs
│   └── PluginLoader.cs
│
├── ScaleDriver.UI/            (Windows Forms)
│   ├── Program.cs
│   ├── MainForm.cs
│   └── FormatEditorForm.cs
│
├── ScaleDriver.Console/       (Console App)
│   └── Program.cs
│
├── ScaleDriver.Plugins/       (Class Library)
│   └── ExampleParsers.cs
│       ├── MettlerToledoParser
│       └── OhausParser
│
└── Documentation/
    ├── README.md
    ├── QUICKSTART.md
    ├── IMPLEMENTATION.md
    └── example-parsers.json
```
