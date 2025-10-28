using System;
using System.Text.RegularExpressions;
using ScaleDriver.Core;

namespace ScaleDriver.Plugins
{
    /// <summary>
    /// Example plugin parser for Mettler Toledo scales
    /// Format: S S    12.50 kg
    /// </summary>
    public class MettlerToledoParser : IWeightFrameParser
    {
        private readonly Regex _regex;

        public string Name => "Mettler Toledo MT-SICS";
        public string Description => "Parser for Mettler Toledo scales using MT-SICS protocol";

        public MettlerToledoParser()
        {
            _regex = new Regex(@"(?<stable>S|D|SD|SI)\s+(?<sign>[+-]?)\s*(?<weight>\d+\.?\d*)\s*(?<unit>\w+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

            int score = 70; // High base score for this specific format

            // Check for Mettler Toledo specific stability indicators
            var stable = match.Groups["stable"].Value;
            if (stable == "S" || stable == "SD")
                score += 15;

            // Check for typical Mettler Toledo units
            var unit = match.Groups["unit"].Value.ToLower();
            if (unit == "kg" || unit == "g" || unit == "lb")
                score += 15;

            return Math.Min(score, 100);
        }

        public WeightResult Parse(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return WeightResult.CreateError("Empty data", data);

            var match = _regex.Match(data);
            if (!match.Success)
                return WeightResult.CreateError("Data does not match Mettler Toledo format", data);

            try
            {
                var weightStr = match.Groups["weight"].Value;
                var sign = match.Groups["sign"].Value;
                var unit = match.Groups["unit"].Value;
                var stableStr = match.Groups["stable"].Value;

                if (!decimal.TryParse(weightStr, out decimal weight))
                    return WeightResult.CreateError($"Could not parse weight: {weightStr}", data);

                if (sign == "-")
                    weight = -weight;

                // S = Stable, D = Dynamic, SD = Stable in dynamic mode, SI = Stable with invalid data
                bool isStable = stableStr == "S" || stableStr == "SD";

                var result = WeightResult.CreateSuccess(weight, unit, isStable);
                result.RawData = data;
                return result;
            }
            catch (Exception ex)
            {
                return WeightResult.CreateError($"Parse error: {ex.Message}", data);
            }
        }
    }

    /// <summary>
    /// Example plugin parser for Ohaus scales
    /// Format: 0.12 kg
    /// </summary>
    public class OhausParser : IWeightFrameParser
    {
        private readonly Regex _regex;

        public string Name => "Ohaus Digital Scale";
        public string Description => "Parser for Ohaus digital scales";

        public OhausParser()
        {
            _regex = new Regex(@"(?<weight>-?\d+\.?\d*)\s*(?<unit>kg|g|lb|oz)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

            // Lower confidence as this is a very generic format
            return 40;
        }

        public WeightResult Parse(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return WeightResult.CreateError("Empty data", data);

            var match = _regex.Match(data);
            if (!match.Success)
                return WeightResult.CreateError("Data does not match Ohaus format", data);

            try
            {
                var weightStr = match.Groups["weight"].Value;
                var unit = match.Groups["unit"].Value;

                if (!decimal.TryParse(weightStr, out decimal weight))
                    return WeightResult.CreateError($"Could not parse weight: {weightStr}", data);

                var result = WeightResult.CreateSuccess(weight, unit, true);
                result.RawData = data;
                return result;
            }
            catch (Exception ex)
            {
                return WeightResult.CreateError($"Parse error: {ex.Message}", data);
            }
        }
    }
}
