using Arbor.Build.Core.GenericExtensions.Boolean;
using Machine.Specifications;

namespace Arbor.Build.Tests.Integration.BoolExtensions
{
    [Subject(typeof(Core.GenericExtensions.Boolean.BoolExtensions))]
    public class when_parsing_null_value_with_default_true
    {
        static bool result_value;

        Because of = () =>
        {
            ((string)null).TryParseBool(out bool result, true);
            result_value = result;
        };

        It should_be_true = () => result_value.ShouldBeTrue();
    }
}
