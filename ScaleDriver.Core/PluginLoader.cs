using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ScaleDriver.Core
{
    /// <summary>
    /// Loads parser plugins from DLL files
    /// </summary>
    public class PluginLoader
    {
        private readonly List<string> _pluginPaths;

        public PluginLoader()
        {
            _pluginPaths = new List<string>();
        }

        /// <summary>
        /// Adds a directory to search for plugins
        /// </summary>
        public void AddPluginPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty", nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Plugin directory not found: {path}");

            if (!_pluginPaths.Contains(path))
            {
                _pluginPaths.Add(path);
            }
        }

        /// <summary>
        /// Loads all parsers from plugin DLLs in the registered paths
        /// </summary>
        public List<IWeightFrameParser> LoadParsers()
        {
            var parsers = new List<IWeightFrameParser>();

            foreach (var path in _pluginPaths)
            {
                try
                {
                    var dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
                    
                    foreach (var dllFile in dllFiles)
                    {
                        try
                        {
                            var loadedParsers = LoadParsersFromAssembly(dllFile);
                            parsers.AddRange(loadedParsers);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle individual DLL loading errors
                            Console.WriteLine($"Failed to load parsers from {dllFile}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to scan directory {path}: {ex.Message}");
                }
            }

            return parsers;
        }

        /// <summary>
        /// Loads parsers from a specific assembly file
        /// </summary>
        public List<IWeightFrameParser> LoadParsersFromAssembly(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentException("Assembly path cannot be empty", nameof(assemblyPath));

            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file not found: {assemblyPath}");

            var parsers = new List<IWeightFrameParser>();

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var parserTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && 
                                typeof(IWeightFrameParser).IsAssignableFrom(t))
                    .ToList();

                foreach (var type in parserTypes)
                {
                    try
                    {
                        // Try to create an instance with default constructor
                        var parser = (IWeightFrameParser)Activator.CreateInstance(type);
                        parsers.Add(parser);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create instance of {type.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load assembly {assemblyPath}", ex);
            }

            return parsers;
        }

        /// <summary>
        /// Gets all registered plugin paths
        /// </summary>
        public IReadOnlyList<string> GetPluginPaths()
        {
            return _pluginPaths.AsReadOnly();
        }

        /// <summary>
        /// Clears all registered plugin paths
        /// </summary>
        public void ClearPluginPaths()
        {
            _pluginPaths.Clear();
        }
    }
}
