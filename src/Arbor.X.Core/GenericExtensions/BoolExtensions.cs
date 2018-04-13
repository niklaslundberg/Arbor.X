﻿using Arbor.X.Core.Parsing;

namespace Arbor.X.Core.GenericExtensions
{
    public static class BoolExtensions
    {
        public static ParseResult<bool> TryParseBool(this string value, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ParseResult<bool>.Create(defaultValue, false, value);
            }

            if (!bool.TryParse(value, out bool parsedValue))
            {
                return ParseResult<bool>.Create(defaultValue, false, value);
            }

            return ParseResult<bool>.Create(parsedValue, true, value);
        }
    }
}
