using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests;

/// <summary>
/// Guards against accidental modification or deletion of existing ChatModel seed data.
/// Rule: existing IDs and their ApiModelName values are immutable — only additions are allowed.
/// When adding a new model to ChatModelsConfiguration, add its ID and ApiModelName here too.
/// </summary>
public class ChatModelSeedTests
{
    // Immutable registry: ID → ApiModelName.
    // Never change existing entries. Only append new ones when a new model is added.
    private static readonly Dictionary<int, string> KnownModels = new()
    {
        { 1, "gpt-3.5-turbo" },
        { 2, "gpt-4" },
        { 3, "gpt-4o" },
        { 4, "gpt-4o-mini" },
        { 5, "gpt-5" },
    };

    private readonly AppTrackDatabaseContext _context;

    public ChatModelSeedTests()
    {
        var options = new DbContextOptionsBuilder<AppTrackDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppTrackDatabaseContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AllKnownChatModels_ShouldExistWithCorrectApiModelName()
    {
        var seeded = await _context.ChatModels.ToListAsync();

        foreach (var (id, expectedApiModelName) in KnownModels)
        {
            var model = seeded.SingleOrDefault(m => m.Id == id);
            model.ShouldNotBeNull($"ChatModel with ID {id} was deleted or its ID was changed.");
            model.ApiModelName.ShouldBe(expectedApiModelName,
                $"ChatModel ID {id} ApiModelName must not be changed (was '{expectedApiModelName}').");
        }
    }

    [Fact]
    public async Task NoChatModel_ShouldBeAddedWithoutBeingRegisteredInKnownModels()
    {
        var seeded = await _context.ChatModels.ToListAsync();

        seeded.Count.ShouldBe(KnownModels.Count,
            $"A new ChatModel was seeded without registering it in {nameof(KnownModels)}. Add the new model's ID and ApiModelName to the dictionary.");
    }
}
