using System;

namespace ScaleDriver.Core
{
    /// <summary>
    /// Interface for weight frame parsers that can parse data from RS232 scales
    /// </summary>
    public interface IWeightFrameParser
    {
        /// <summary>
        /// Gets the name of the parser
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the parser
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Determines if this parser can parse the given data
        /// </summary>
        /// <param name="data">Raw data to check</param>
        /// <returns>True if the parser can handle this data</returns>
        bool CanParse(string data);

        /// <summary>
        /// Gets confidence score (0-100) for parsing the given data
        /// Higher score means more confident
        /// </summary>
        /// <param name="data">Raw data to check</param>
        /// <returns>Confidence score from 0 to 100</returns>
        int GetConfidenceScore(string data);

        /// <summary>
        /// Parses the weight data from the frame
        /// </summary>
        /// <param name="data">Raw frame data</param>
        /// <returns>Parsed weight result</returns>
        WeightResult Parse(string data);
    }

    /// <summary>
    /// Result of parsing a weight frame
    /// </summary>
    public class WeightResult
    {
        public bool Success { get; set; }
        public decimal Weight { get; set; }
        public string Unit { get; set; }
        public string ErrorMessage { get; set; }
        public string RawData { get; set; }
        public bool IsStable { get; set; }

        public static WeightResult CreateSuccess(decimal weight, string unit, bool isStable = true)
        {
            return new WeightResult
            {
                Success = true,
                Weight = weight,
                Unit = unit,
                IsStable = isStable
            };
        }

        public static WeightResult CreateError(string errorMessage, string rawData = null)
        {
            return new WeightResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                RawData = rawData
            };
        }
    }
}
