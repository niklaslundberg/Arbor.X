﻿using System;
using JetBrains.Annotations;

namespace Arbor.Build.Core.BuildVariables
{
    public class BuildVariable : IVariable
    {
        public BuildVariable([NotNull] string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }

        public override string ToString()
        {
            return this.DisplayValue();
        }
    }
}
