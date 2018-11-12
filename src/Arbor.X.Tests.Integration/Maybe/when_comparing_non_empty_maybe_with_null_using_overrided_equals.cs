using Machine.Specifications;

namespace Arbor.Build.Tests.Integration.Maybe
{
    public class when_comparing_non_empty_maybe_with_null_using_overrided_equals
    {
        static bool equal;
        Because of = () => equal = new Defensive.Maybe<string>("a string").Equals(null);

        It should_return_false = () => equal.ShouldBeFalse();
    }
}
