using AppTrack.Domain;
using Shouldly;

namespace AppTrack.Application.UnitTests.Domain;

public class DefaultPromptFactoryTests
{
    [Fact]
    public void Create_ShouldReturnDefaultPrompt_WhenAllArgumentsAreValid()
    {
        var result = DefaultPrompt.Create("Anschreiben", "Template text", "de");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Anschreiben");
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
        Should.Throw<ArgumentNullException>(() => DefaultPrompt.Create("Name", null, "de"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenLanguageIsNull()
    {
        Should.Throw<ArgumentNullException>(() => DefaultPrompt.Create("Name", "template", null));
    }
}
