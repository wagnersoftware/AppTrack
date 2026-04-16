// DTOs für OpenAI-Response
namespace AppTrack.Infrastructure.AiTextGeneration.OpAiModels
{
    public class ChatCompletionResponse
    {
        public List<Choice>? Choices { get; set; }
    }
}
