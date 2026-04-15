using AppTrack.Domain;
using Shouldly;

namespace AppTrack.Application.UnitTests.Domain;

public class BuiltInPromptFactoryTests
{
    [Fact]
    public void Create_ShouldReturnBuiltInPrompt_WhenAllArgumentsAreValid()
    {
        var result = BuiltInPrompt.Create("builtIn_Cover_Letter", "Template text");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("builtIn_Cover_Letter");
        result.PromptTemplate.ShouldBe("Template text");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create(null, "template"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenPromptTemplateIsNull()
    {
        Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("builtIn_Name", null));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameDoesNotStartWithDefaultPrefix()
    {
        Should.Throw<ArgumentException>(() => BuiltInPrompt.Create("Cover_Letter", "template"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameContainsSpace()
    {
        Should.Throw<ArgumentException>(() => BuiltInPrompt.Create("builtIn_Cover Letter", "template"));
    }
}
