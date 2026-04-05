using AppTrack.Domain;
using Shouldly;

namespace AppTrack.Application.UnitTests.Domain;

public class DefaultPromptFactoryTests
{
    [Fact]
    public void Create_ShouldReturnDefaultPrompt_WhenAllArgumentsAreValid()
    {
        var result = DefaultPrompt.Create("Default_Cover_Letter", "Template text", "de");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Default_Cover_Letter");
        result.PromptTemplate.ShouldBe("Template text");
        result.Language.ShouldBe("de");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => DefaultPrompt.Create(null, "template", "de"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenPromptTemplateIsNull()
    {
        Should.Throw<ArgumentNullException>(() => DefaultPrompt.Create("Default_Name", null, "de"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenLanguageIsNull()
    {
        Should.Throw<ArgumentNullException>(() => DefaultPrompt.Create("Default_Name", "template", null));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameDoesNotStartWithDefaultPrefix()
    {
        Should.Throw<ArgumentException>(() => DefaultPrompt.Create("Cover_Letter", "template", "de"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameContainsSpace()
    {
        Should.Throw<ArgumentException>(() => DefaultPrompt.Create("Default_Cover Letter", "template", "de"));
    }
}
