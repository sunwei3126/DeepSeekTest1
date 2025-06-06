using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;

namespace DeepSeekTest
{

  public sealed class DataModel
  {
    [VectorStoreKey]
    [TextSearchResultName]
    public string Key { get; init; }

    [VectorStoreData]
    [TextSearchResultValue]
    public string Text { get; init; }

    [VectorStoreVector(768)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
  }

  public class VectorStoreTest
  {
   
    public static async Task Test()
    {
      // Create a kernel with deepseek chat completion
      var DEEPSEEK_API_KEY = "";
      var GOOGLE_API_KEY = "AIzaSyAw_aNaAlf9F-wtgdZ12yt4zjkfEJNrNtU";
      // Prepare and build kernel  
      var builder = Kernel.CreateBuilder();

      builder.AddOpenAIChatCompletion(
          modelId: "deepseek-chat",
          endpoint: new Uri("https://api.deepseek.com"),
          apiKey: DEEPSEEK_API_KEY
      );



#pragma warning disable SKEXP0070 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
      builder.AddGoogleAIEmbeddingGenerator(
          modelId: "text-embedding-004",       // Name of the embedding model, e.g. "models/text-embedding-004".
          apiKey: GOOGLE_API_KEY
      );
#pragma warning restore SKEXP0070 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

   
      var kernel = builder.Build();
      var textEmbeddingGenerator= kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
      // Create a in memory VectorStore object
      var vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions { EmbeddingGenerator = textEmbeddingGenerator });

      // Choose a collection from the database and specify the type of key and record stored in it via Generic parameters.
      var collection = vectorStore.GetCollection<string, DataModel>("sk-entities");

      //  var textEmbeddingGenerationService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
      //  var  embeddings =
      // await textEmbeddingGenerationService.GenerateAsync(
      //[
      //    "你好",
      //    "你好啊"
      //]);

      await collection.EnsureCollectionExistsAsync();
      for (int i = 0; i < 5; i++)
      {
        var text = "设备" + i.ToString();
        var embedding = await textEmbeddingGenerator.GenerateVectorAsync(text);
        await collection.UpsertAsync(new DataModel { Key = i.ToString(), Text = text, DescriptionEmbedding = embedding });
        await Task.Delay(100);
      }

      ReadOnlyMemory<float> searchVector = await textEmbeddingGenerator.GenerateVectorAsync("设备ctorAsyn   -4");

      // Do the search.
      var searchResult = collection.SearchAsync(searchVector, top: 2);

      // Inspect the returned hotel.
      await foreach (var record in searchResult)
      {
        Console.WriteLine("Found hotel description: " + record.Record.Text);
  
        Console.WriteLine("Found record score: " + record.Score);
      }



#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
      // Create a text search instance using the vector store collection.
      var textSearch = new VectorStoreTextSearch<DataModel>(collection);
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

      // Build a text search plugin with vector store search and add to the kernel
      var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
      kernel.Plugins.Add(searchPlugin);

      ChatCompletionAgent agent =
          new()
          {
            Name = "SK-Assistant",
            Instructions = "You are a helpful assistant.",
            Kernel = kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
          };


      while (true)
      {
        Console.Write("\nWhat do you need help with? ");
        var userInput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
        {
          Console.WriteLine("👋 Exiting...");
          break;
        }

        await foreach (AgentResponseItem<ChatMessageContent> response
            in agent.InvokeAsync(userInput))
        {
          Console.WriteLine(response.Message);
        }

      }
    }
  }

}


