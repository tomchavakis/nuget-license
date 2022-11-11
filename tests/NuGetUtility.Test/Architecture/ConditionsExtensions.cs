using NetArchTest.Rules;

namespace NuGetUtility.Test.Architecture
{
    internal static class ConditionsExtensions
    {
        public static void Assert(this ConditionList conditions, string message = "Architecture rule broken.")
        {
            var ruleResult = conditions.GetResult();
            var failingTypeNames = string.Join(Environment.NewLine, ruleResult.FailingTypeNames ?? Array.Empty<string>());
            NUnit.Framework.Assert.True(ruleResult.IsSuccessful, $"{message}{Environment.NewLine}Offending types:{Environment.NewLine}{failingTypeNames}");
        }
    }
}
