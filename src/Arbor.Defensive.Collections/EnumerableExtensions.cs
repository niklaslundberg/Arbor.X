﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Arbor.Defensive.Collections
{
    public static class EnumerableExtensions
    {
        public static ImmutableArray<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (enumerable is ImmutableArray<T> array)
            {
                if (array.IsDefault)
                {
                    return ImmutableArray<T>.Empty;
                }

                return array;
            }

            ImmutableArray<T> immutableArray = enumerable.ToImmutableArray();

            return immutableArray;
        }

        public static ImmutableArray<T> SafeToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return ImmutableArray<T>.Empty;
            }

            if (enumerable is ImmutableArray<T> array)
            {
                if (array.IsDefault)
                {
                    return ImmutableArray<T>.Empty;
                }

                return array;
            }

            ImmutableArray<T> immutableArray = enumerable.ToImmutableArray();

            return immutableArray;
        }

        public static ImmutableArray<T> ValueToImmutableArray<T>(this T item)
        {
            ImmutableArray<T> immutableArray = new[] { item }.ToImmutableArray();

            return immutableArray;
        }
    }
}
