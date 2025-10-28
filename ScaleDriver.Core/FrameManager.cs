using System;
using System.Collections.Generic;
using System.Linq;

namespace ScaleDriver.Core
{
    /// <summary>
    /// Manages frame parsing with multiple parsers using auto-detection
    /// </summary>
    public class FrameManager
    {
        private readonly List<IWeightFrameParser> _parsers;
        private IWeightFrameParser _lastSuccessfulParser;

        public event EventHandler<WeightParsedEventArgs> WeightParsed;
        public event EventHandler<ParseErrorEventArgs> ParseError;

        public IReadOnlyList<IWeightFrameParser> Parsers => _parsers.AsReadOnly();

        public FrameManager()
        {
            _parsers = new List<IWeightFrameParser>();
        }

        /// <summary>
        /// Adds a parser to the manager
        /// </summary>
        public void AddParser(IWeightFrameParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            _parsers.Add(parser);
        }

        /// <summary>
        /// Removes a parser from the manager
        /// </summary>
        public void RemoveParser(IWeightFrameParser parser)
        {
            _parsers.Remove(parser);
        }

        /// <summary>
        /// Clears all parsers
        /// </summary>
        public void ClearParsers()
        {
            _parsers.Clear();
            _lastSuccessfulParser = null;
        }

        /// <summary>
        /// Processes raw frame data using auto-detection
        /// </summary>
        public WeightResult ProcessFrame(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return WeightResult.CreateError("Empty data received");
            }

            if (_parsers.Count == 0)
            {
                return WeightResult.CreateError("No parsers available");
            }

            // Try last successful parser first for optimization
            if (_lastSuccessfulParser != null && _lastSuccessfulParser.CanParse(data))
            {
                try
                {
                    var result = _lastSuccessfulParser.Parse(data);
                    if (result.Success)
                    {
                        OnWeightParsed(result, _lastSuccessfulParser);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    OnParseError(data, _lastSuccessfulParser, ex);
                }
            }

            // Auto-detect the best parser using confidence scores
            var candidates = _parsers
                .Where(p => p.CanParse(data))
                .Select(p => new { Parser = p, Score = p.GetConfidenceScore(data) })
                .OrderByDescending(x => x.Score)
                .ToList();

            foreach (var candidate in candidates)
            {
                try
                {
                    var result = candidate.Parser.Parse(data);
                    if (result.Success)
                    {
                        _lastSuccessfulParser = candidate.Parser;
                        OnWeightParsed(result, candidate.Parser);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    OnParseError(data, candidate.Parser, ex);
                }
            }

            // If no parser could handle the data
            var errorResult = WeightResult.CreateError($"No parser could parse the data", data);
            OnWeightParsed(errorResult, null);
            return errorResult;
        }

        protected virtual void OnWeightParsed(WeightResult result, IWeightFrameParser parser)
        {
            WeightParsed?.Invoke(this, new WeightParsedEventArgs
            {
                Result = result,
                Parser = parser
            });
        }

        protected virtual void OnParseError(string data, IWeightFrameParser parser, Exception exception)
        {
            ParseError?.Invoke(this, new ParseErrorEventArgs
            {
                Data = data,
                Parser = parser,
                Exception = exception
            });
        }
    }

    public class WeightParsedEventArgs : EventArgs
    {
        public WeightResult Result { get; set; }
        public IWeightFrameParser Parser { get; set; }
    }

    public class ParseErrorEventArgs : EventArgs
    {
        public string Data { get; set; }
        public IWeightFrameParser Parser { get; set; }
        public Exception Exception { get; set; }
    }
}
