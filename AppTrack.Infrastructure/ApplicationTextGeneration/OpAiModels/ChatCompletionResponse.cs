// DTOs für OpenAI-Response
namespace AppTrack.Infrastructure.ApplicationTextGeneration.OpAiModels
{
    public class ChatCompletionResponse
    {
        public List<Choice>? Choices { get; set; }
    }
}