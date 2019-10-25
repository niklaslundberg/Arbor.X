using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Arbor.Defensive;

namespace Arbor.Build.Core.BuildVariables
{
    public static class BuildVariableExtensions
    {
        public static bool HasKey(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key) => buildVariables.Any(
            bv => bv.Key.Equals(
                key,
                StringComparison.OrdinalIgnoreCase));

        public static IVariable GetVariable(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key) => buildVariables.Single(
            bv => bv.Key.Equals(
                key,
                StringComparison.OrdinalIgnoreCase));

        public static Maybe<IVariable> GetOptionalVariable(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key)
        {
            IVariable variable = buildVariables.SingleOrDefault(
                bv => bv.Key.Equals(
                    key,
                    StringComparison.OrdinalIgnoreCase));

            if (variable is null)
            {
                return Maybe<IVariable>.Empty();
            }

            return new Maybe<IVariable>(variable);
        }

        public static string? GetVariableValueOrDefault(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key,
            string? defaultValue)
        {
            if (!buildVariables.HasKey(key))
            {
                return defaultValue;
            }

            return buildVariables.GetVariable(key).Value;
        }

        public static bool GetBooleanByKey(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key,
            bool defaultValue = false)
        {
            if (!buildVariables.HasKey(key))
            {
                return defaultValue;
            }

            string? value = buildVariables.GetVariableValueOrDefault(
                key,
                string.Empty);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (!bool.TryParse(
                value,
                out bool parsed))
            {
                return defaultValue;
            }

            return parsed;
        }

        public static bool? GetOptionalBooleanByKey(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key)
        {
            if (!buildVariables.HasKey(key))
            {
                return null;
            }

            string? value = buildVariables.GetVariableValueOrDefault(
                key,
                default);

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!bool.TryParse(
                value,
                out bool parsed))
            {
                return null;
            }

            return parsed;
        }

        public static int GetInt32ByKey(
            this IReadOnlyCollection<IVariable> buildVariables,
            string key,
            int defaultValue = default,
            int? minValue = null)
        {
            int? returnValue = null;

            if (buildVariables.HasKey(key))
            {
                string? value = buildVariables.GetVariableValueOrDefault(
                    key,
                    defaultValue.ToString(CultureInfo.InvariantCulture));

                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out int parsed))
                    {
                        returnValue = parsed;
                    }
                }
            }

            if (!returnValue.HasValue)
            {
                returnValue = defaultValue;
            }

            if (returnValue < minValue)
            {
                returnValue = minValue;
            }

            return returnValue.Value;
        }

        public static bool GetValueOrDefault(
            this IVariable variable,
            bool defaultValue = false)
        {
            if (variable == null)
            {
                return defaultValue;
            }

            if (string.IsNullOrWhiteSpace(variable.Value))
            {
                return defaultValue;
            }

            if (!bool.TryParse(
                variable.Value,
                out bool parsed))
            {
                return defaultValue;
            }

            return parsed;
        }
    }
}