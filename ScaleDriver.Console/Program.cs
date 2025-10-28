using System;
using System.Threading;
using ScaleDriver.Core;

namespace ScaleDriver.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Scale Driver Console Application ===");
            Console.WriteLine("RS232 Weight Reader with Auto-Detection\n");

            // Create components
            var serialReader = new SerialReader();
            var frameManager = new FrameManager();

            // Add default parsers
            AddDefaultParsers(frameManager);

            // Wire up events
            serialReader.DataReceived += (sender, e) =>
            {
                Console.WriteLine($"[RX] {e.Data}");
                var result = frameManager.ProcessFrame(e.Data);
            };

            serialReader.ErrorOccurred += (sender, e) =>
            {
                Console.WriteLine($"[ERROR] {e.Exception.Message}");
            };

            frameManager.WeightParsed += (sender, e) =>
            {
                if (e.Result.Success)
                {
                    var parserName = e.Parser?.Name ?? "Unknown";
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[WEIGHT] {e.Result.Weight} {e.Result.Unit} " +
                                    $"(Stable: {e.Result.IsStable}) - Parser: {parserName}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[PARSE ERROR] {e.Result.ErrorMessage}");
                    Console.ResetColor();
                }
            };

            // Show menu
            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Connect to serial port");
                Console.WriteLine("2. Test parser with sample data");
                Console.WriteLine("3. Add custom regex parser");
                Console.WriteLine("4. List parsers");
                Console.WriteLine("5. Load plugins");
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConnectToSerialPort(serialReader);
                        break;
                    case "2":
                        TestParser(frameManager);
                        break;
                    case "3":
                        AddCustomParser(frameManager);
                        break;
                    case "4":
                        ListParsers(frameManager);
                        break;
                    case "5":
                        LoadPlugins(frameManager);
                        break;
                    case "0":
                        if (serialReader.IsConnected)
                        {
                            serialReader.Close();
                        }
                        serialReader.Dispose();
                        return;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }
        }

        static void AddDefaultParsers(FrameManager frameManager)
        {
            // Generic CSV format: ST,GS,+00012.50,kg
            var csvParser = new RegexParser(
                "Generic CSV",
                @"(?<stable>[A-Z]{2}),(?<mode>[A-Z]{2}),(?<weight>[+-]?\d+\.?\d*),(?<unit>\w+)",
                "weight", "unit", "stable",
                "Generic CSV format with stability, mode, weight, and unit"
            );
            frameManager.AddParser(csvParser);

            // Simple weight format: 12.50 kg
            var simpleParser = new RegexParser(
                "Simple Weight",
                @"(?<weight>\d+\.?\d*)\s*(?<unit>kg|g|lb|oz)",
                "weight", "unit", null,
                "Simple weight format: value followed by unit"
            );
            frameManager.AddParser(simpleParser);

            // Toledo format
            var toledoParser = new RegexParser(
                "Toledo Format",
                @"(?<stable>ST|US)\s+(?<weight>[+-]?\d+\.?\d*)\s*(?<unit>\w+)",
                "weight", "unit", "stable",
                "Toledo scale format"
            );
            frameManager.AddParser(toledoParser);

            Console.WriteLine($"Added {frameManager.Parsers.Count} default parsers");
        }

        static void ConnectToSerialPort(SerialReader serialReader)
        {
            if (serialReader.IsConnected)
            {
                Console.WriteLine("Already connected. Disconnect first.");
                return;
            }

            Console.Write("Enter COM port (e.g., COM1): ");
            var port = Console.ReadLine();

            Console.Write("Enter baud rate (default 9600): ");
            var baudRateStr = Console.ReadLine();
            var baudRate = string.IsNullOrWhiteSpace(baudRateStr) ? 9600 : int.Parse(baudRateStr);

            try
            {
                serialReader.Open(port, baudRate);
                serialReader.StartReading();
                Console.WriteLine($"Connected to {port} at {baudRate} baud");
                Console.WriteLine("Listening for data... (Press Ctrl+C to stop)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
            }
        }

        static void TestParser(FrameManager frameManager)
        {
            Console.WriteLine("\nTest data samples:");
            Console.WriteLine("1. ST,GS,+00012.50,kg");
            Console.WriteLine("2. 25.75 kg");
            Console.WriteLine("3. ST 100.25 lb");
            Console.WriteLine("4. US,NT,-00005.00,g");
            Console.Write("\nEnter test data: ");
            
            var testData = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(testData))
                return;

            Console.WriteLine($"\nTesting: {testData}");
            var result = frameManager.ProcessFrame(testData);
            
            if (!result.Success)
            {
                Console.WriteLine("(Already logged via event handler)");
            }
        }

        static void AddCustomParser(FrameManager frameManager)
        {
            Console.Write("\nEnter parser name: ");
            var name = Console.ReadLine();

            Console.Write("Enter regex pattern: ");
            var pattern = Console.ReadLine();

            Console.Write("Weight group name (default: weight): ");
            var weightGroup = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(weightGroup))
                weightGroup = "weight";

            Console.Write("Unit group name (default: unit): ");
            var unitGroup = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(unitGroup))
                unitGroup = "unit";

            Console.Write("Stability group name (default: stable): ");
            var stabilityGroup = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(stabilityGroup))
                stabilityGroup = "stable";

            try
            {
                var parser = new RegexParser(name, pattern, weightGroup, unitGroup, stabilityGroup);
                frameManager.AddParser(parser);
                Console.WriteLine($"Parser '{name}' added successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add parser: {ex.Message}");
            }
        }

        static void ListParsers(FrameManager frameManager)
        {
            Console.WriteLine($"\nRegistered Parsers ({frameManager.Parsers.Count}):");
            for (int i = 0; i < frameManager.Parsers.Count; i++)
            {
                var parser = frameManager.Parsers[i];
                Console.WriteLine($"{i + 1}. {parser.Name} - {parser.Description}");
            }
        }

        static void LoadPlugins(FrameManager frameManager)
        {
            Console.Write("\nEnter plugin directory path: ");
            var path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Invalid path");
                return;
            }

            try
            {
                var pluginLoader = new PluginLoader();
                pluginLoader.AddPluginPath(path);
                var parsers = pluginLoader.LoadParsers();

                foreach (var parser in parsers)
                {
                    frameManager.AddParser(parser);
                }

                Console.WriteLine($"Loaded {parsers.Count} parser(s) from plugins");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugins: {ex.Message}");
            }
        }
    }
}
