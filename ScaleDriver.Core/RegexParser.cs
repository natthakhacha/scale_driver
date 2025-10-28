using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ScaleDriver.Core
{
    /// <summary>
    /// Regex-based parser for weight frames
    /// Supports both ASCII and binary formats through regex configuration
    /// </summary>
    public class RegexParser : IWeightFrameParser
    {
        private readonly Regex _regex;
        private readonly string _weightGroup;
        private readonly string _unitGroup;
        private readonly string _stabilityGroup;

        public string Name { get; }
        public string Description { get; }
        public string Pattern { get; }

        /// <summary>
        /// Creates a regex parser with specified pattern
        /// </summary>
        /// <param name="name">Parser name</param>
        /// <param name="pattern">Regex pattern to match weight frames</param>
        /// <param name="weightGroup">Name of the capturing group for weight value</param>
        /// <param name="unitGroup">Name of the capturing group for unit (optional)</param>
        /// <param name="stabilityGroup">Name of the capturing group for stability indicator (optional)</param>
        /// <param name="description">Parser description</param>
        public RegexParser(string name, string pattern, string weightGroup = "weight",
            string unitGroup = "unit", string stabilityGroup = "stable", string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Pattern cannot be empty", nameof(pattern));
            if (string.IsNullOrWhiteSpace(weightGroup))
                throw new ArgumentException("Weight group name cannot be empty", nameof(weightGroup));

            Name = name;
            Pattern = pattern;
            Description = description ?? $"Regex parser: {name}";
            _weightGroup = weightGroup;
            _unitGroup = unitGroup;
            _stabilityGroup = stabilityGroup;

            try
            {
                _regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid regex pattern: {ex.Message}", nameof(pattern), ex);
            }
        }

        public bool CanParse(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return false;

            return _regex.IsMatch(data);
        }

        public int GetConfidenceScore(string data)
        {
            if (!CanParse(data))
                return 0;

            var match = _regex.Match(data);
            if (!match.Success)
                return 0;

            int score = 50; // Base score for matching pattern

            // Bonus points for having weight group
            if (match.Groups[_weightGroup].Success)
                score += 30;

            // Bonus points for having unit group
            if (!string.IsNullOrWhiteSpace(_unitGroup) && match.Groups[_unitGroup].Success)
                score += 10;

            // Bonus points for having stability indicator
            if (!string.IsNullOrWhiteSpace(_stabilityGroup) && match.Groups[_stabilityGroup].Success)
                score += 10;

            return Math.Min(score, 100);
        }

        public WeightResult Parse(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return WeightResult.CreateError("Empty data", data);

            var match = _regex.Match(data);
            if (!match.Success)
                return WeightResult.CreateError("Data does not match pattern", data);

            // Extract weight
            var weightGroup = match.Groups[_weightGroup];
            if (!weightGroup.Success)
                return WeightResult.CreateError($"Weight group '{_weightGroup}' not found in match", data);

            var weightStr = weightGroup.Value.Trim();
            if (!decimal.TryParse(weightStr, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal weight))
            {
                return WeightResult.CreateError($"Could not parse weight value: {weightStr}", data);
            }

            // Extract unit (optional)
            string unit = "kg"; // Default unit
            if (!string.IsNullOrWhiteSpace(_unitGroup))
            {
                var unitGroup = match.Groups[_unitGroup];
                if (unitGroup.Success && !string.IsNullOrWhiteSpace(unitGroup.Value))
                {
                    unit = unitGroup.Value.Trim();
                }
            }

            // Extract stability (optional)
            bool isStable = true; // Default to stable
            if (!string.IsNullOrWhiteSpace(_stabilityGroup))
            {
                var stabilityGroup = match.Groups[_stabilityGroup];
                if (stabilityGroup.Success)
                {
                    var stabilityValue = stabilityGroup.Value.Trim().ToUpper();
                    // Consider various stability indicators
                    isStable = stabilityValue == "S" || stabilityValue == "ST" || 
                               stabilityValue == "STABLE" || stabilityValue == "1" ||
                               stabilityValue == "OK";
                }
            }

            var result = WeightResult.CreateSuccess(weight, unit, isStable);
            result.RawData = data;
            return result;
        }
    }
}
