using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using System.ComponentModel;

namespace DeepSeekTest
{

  sealed class MenuPlugin
  {
    [KernelFunction, Description("Provides a list of specials from the menu.")]
    public string GetSpecials() =>
        """
        Special Soup: Clam Chowder
        Special Salad: Cobb Salad
        Special Drink: Chai Tea
        """;

    [KernelFunction, Description("Provides the price of the requested menu item.")]
    public string GetItemPrice(
        [Description("The name of the menu item.")]
        string menuItem) =>
        "$9.99";
  }

  public class TaskPlugin
  {
    private readonly List<string> _tasks = new();

    [KernelFunction("add_task")]
    [Description("Adds a new task to the to-do list.")]
    public void AddTask([Description("The task description.")] string task)
    {
      _tasks.Add(task);
      Console.WriteLine($"📝 Task added: {task}");
    }

    [KernelFunction("show_tasks")]
    [Description("Returns a list of all tasks currently in the to-do list.")]
    public List<string> ShowTasks()
    {
      return _tasks.Count > 0 ? _tasks : new List<string> { "📌 No tasks found." };
    }

    [KernelFunction("complete_task")]
    [Description("Marks a task as completed and removes it from the list.")]
    public bool CompleteTask([Description("The task to complete.")] string task)
    {
      if (_tasks.Remove(task))
      {
        Console.WriteLine($"✅ Task completed: {task}");
        return true;
      }

      Console.WriteLine($"⚠️ Task not found: {task}");
      return false;
    }
  }


  public class AgentWithPlugin
  {
    public static async Task Test()
    {
      var DEEPSEEK_API_KEY = "sk-835453322a2b4bfb96ef8294411372e9";
      var builder = Kernel.CreateBuilder();
      builder.Services.AddOpenAIChatCompletion(
               modelId: "deepseek-chat",
               endpoint: new Uri("https://api.deepseek.com"),
               apiKey: DEEPSEEK_API_KEY
           );
      Kernel kernel = builder.Build();

      kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());
      kernel.Plugins.Add(KernelPluginFactory.CreateFromType<TaskPlugin>("TODO"));

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
