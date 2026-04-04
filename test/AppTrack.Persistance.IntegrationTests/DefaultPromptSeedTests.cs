using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests;

public class DefaultPromptSeedTests
{
    private readonly AppTrackDatabaseContext _context;

    public DefaultPromptSeedTests()
    {
        var options = new DbContextOptionsBuilder<AppTrackDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppTrackDatabaseContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AllDefaultPrompts_ShouldStartWithDefaultPrefix()
    {
        var prompts = await _context.DefaultPrompts.ToListAsync();

        prompts.ShouldNotBeEmpty();
        prompts.ShouldAllBe(p => p.Name.StartsWith("Default_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AllDefaultPrompts_ShouldNotContainSpaces()
    {
        var prompts = await _context.DefaultPrompts.ToListAsync();

        prompts.ShouldNotBeEmpty();
        prompts.ShouldAllBe(p => !p.Name.Contains(' '));
    }
}
