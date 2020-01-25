using Machine.Specifications;

namespace Arbor.Build.Tests.Integration.Maybe
{
    public class when_comparing_two_default_maybes_with_not_equals_operator
    {
        static bool equal;
        Because of = () => equal = default != default(Defensive.Maybe<string>);

        It should_return_true = () => equal.ShouldBeTrue();
    }
}