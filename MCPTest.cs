using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using ModelContextProtocol.Client;

namespace DeepSeekTest
{
  public class MCPTest
  {
    public static async void Test()
    {
      var DEEPSEEK_API_KEY = "sk-835453322a2b4bfb96ef8294411372e9";
      // Prepare and build kernel  
      var builder = Kernel.CreateBuilder();
   
      builder.Services.AddOpenAIChatCompletion(
          modelId: "deepseek-chat",
          endpoint: new Uri("https://api.deepseek.com"),
          apiKey: DEEPSEEK_API_KEY
      );
      Kernel kernel = builder.Build();

      // Create an MCPClient for the GitHub server
      await using IMcpClient mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
      {
        Name = "GitHub",
        Command = "npx",
        Arguments = ["-y", "@modelcontextprotocol/server-github"],
      }));

      var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
      kernel.Plugins.AddFromFunctions("Github", tools.Select(tool => tool.AsKernelFunction()));
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

      //      OpenAIPromptExecutionSettings executionSettings = new()
      //      {
      //        Temperature = 0,
      //        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { AllowParallelCalls = true })
      //      };

      //      //test using Github tools
      //      var prompt = "Summarize the last four commits to the microsoft/semantic-kernel repository?";
      //      var result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
      //      Console.WriteLine($"\n\n{prompt}\n{result}");


      // Define the agent
      // Define the agent
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
      ChatCompletionAgent agent =
          new()
          {
            Instructions = "Answer questions about GitHub repositories.",
            Name = "GitHubAgent",
            Kernel = kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) }),
          };
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

      // Respond to user input, invoking functions where appropriate.
      ChatMessageContent response = await agent.InvokeAsync("Summarize the last four commits to the microsoft/semantic-kernel repository?").FirstAsync();
      Console.WriteLine($"\n\nResponse from GitHubAgent:\n{response.Content}");


    }
  }
}
