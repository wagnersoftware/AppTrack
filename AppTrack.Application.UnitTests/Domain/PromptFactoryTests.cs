using AppTrack.Domain;
using Shouldly;

namespace AppTrack.Application.UnitTests.Domain;

public class PromptFactoryTests
{
    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => Prompt.Create(null, "template"));
    }

    [Fact]
    public void Create_WithNullTemplate_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => Prompt.Create("My Prompt", (string?)null));
    }

    [Fact]
    public void Create_WithValidArgs_ShouldReturnPrompt()
    {
        var prompt = Prompt.Create("My Prompt", "Hello {name}");
        prompt.Name.ShouldBe("My Prompt");
        prompt.PromptTemplate.ShouldBe("Hello {name}");
    }
}
