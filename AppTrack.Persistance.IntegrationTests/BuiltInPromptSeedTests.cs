using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests;

public class BuiltInPromptSeedTests
{
    private readonly AppTrackDatabaseContext _context;

    public BuiltInPromptSeedTests()
    {
        var options = new DbContextOptionsBuilder<AppTrackDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppTrackDatabaseContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AllBuiltInPrompts_ShouldStartWithDefaultPrefix()
    {
        var prompts = await _context.BuiltInPrompts.ToListAsync();

        prompts.ShouldNotBeEmpty();
        prompts.ShouldAllBe(p => p.Name.StartsWith("Default_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AllBuiltInPrompts_ShouldNotContainSpaces()
    {
        var prompts = await _context.BuiltInPrompts.ToListAsync();

        prompts.ShouldNotBeEmpty();
        prompts.ShouldAllBe(p => !p.Name.Contains(' '));
    }
}
