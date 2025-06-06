using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DeepSeekTest
{
  public class DeepSeekTest
  {  /// <summary>
     /// https://devblogs.microsoft.com/semantic-kernel/using-deepseek-models-in-semantic-kernel/
     /// </summary>
    public static async void TeskConnect()
    {
      var DEEPSEEK_API_KEY = "sk-835453322a2b4bfb96ef8294411372e9";

        #pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
      OpenAIChatCompletionService chatCompletionService = new(
          "deepseek-chat",    //"deepseek-reasoner" or "deepseek-chat"
          new Uri("https://api.deepseek.com"),
          DEEPSEEK_API_KEY);
          #pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

      var chatHistory = new ChatHistory();
      chatHistory.AddUserMessage("Hello, how are you?");

      var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
      Console.WriteLine(reply);
    }
  }
}
