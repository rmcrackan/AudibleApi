namespace ExceptionExtensionsTests;

[TestClass]
public class ToJson
{
	[TestMethod]
	public void NestedException()
	{
		// Don't actually throw. That would create a stack trace in the test output with specific local paths which would make the test more complicated
		var ex = new InvalidOperationException("Outer Exception",
			new Exception("Inner Exception 1",
			new Exception("Inner Exception 2")));

		var expected =
			"""
                {
                  "error": "my message",
                  "error_message": "Outer Exception",
                  "error_stack_trace": null,
                  "inner_exception_1": {
                    "error_message": "Inner Exception 1",
                    "error_stack_trace": null
                  },
                  "inner_exception_2": {
                    "error_message": "Inner Exception 2",
                    "error_stack_trace": null
                  }
                }
                """;

		ExceptionExtensions
			.ToJson(ex, "my message")
			.ToString(Formatting.Indented)
			.ShouldBe(expected);
	}
}
