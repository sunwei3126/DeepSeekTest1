using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DeepSeekTest
{
  internal class Program
  {
    static void Main(string[] args)
    {
      //DeepSeekTest.TeskConnect();
      //MCPTest.Test();
      //AgentWithPlugin.Test().GetAwaiter().GetResult();
      VectorStoreTest.Test().GetAwaiter().GetResult();
      Console.ReadLine();
    }
  }
}
