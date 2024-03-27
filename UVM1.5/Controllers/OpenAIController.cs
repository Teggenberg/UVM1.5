using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenAI_API;
using OpenAI_API.Completions;
using UVM1._5.Models;

namespace UVM1._5.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class OpenAIController : ControllerBase
    {
        private string _key = "";

        public OpenAIController()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false);
            IConfiguration configuration = builder.Build();

            _key = configuration.GetConnectionString("OpenAi");
            
        }
        //[HttpGet]
        //[Route("UseChatGPT")]
        public async Task<string> UseChatGPT(string query)
        {
            string outputResult = "";
            OpenAIAPI openai = new OpenAIAPI(_key);
            CompletionRequest completionRequest = new CompletionRequest();
            completionRequest.Prompt = query;
            completionRequest.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
            completionRequest.MaxTokens = 512;

            var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

            foreach (var completion in completions.Completions)
            {
                outputResult += completion.Text;
                System.Diagnostics.Debug.WriteLine(outputResult);
            }
            return outputResult;
            //return Ok(outputResult);

        }

        public async Task<string> GetDescription(string item)
        {
            OpenAIAPI openai = new OpenAIAPI(_key);

            var chat = openai.Chat.CreateConversation();

            chat.AppendSystemMessage("You are assisting to generate a short product description to advertise pre-owned musical instruments for sale. You will be provided a specific model, and need to genreate a product summary of around 600 characters.");
            chat.AppendUserInput(item);

            

            var response = await chat.GetResponseFromChatbotAsync();

            return response;

        }

        public async Task<string> GetCategory(List<string> categories, string item)
        {

            OpenAIAPI openai = new OpenAIAPI(_key);

            var chat = openai.Chat.CreateConversation();

            chat.AppendSystemMessage("Using only one of the categories in the following list, please tell me which categpry the provided item falls best into. Please respond with Only a category selection included in the following list.");
            chat.AppendSystemMessage($"The Listcontains the following: {categories[0]}");
            for (int i = 1; i <categories.Count(); i++)       
            {
                chat.AppendSystemMessage($", {categories[i]}");
            }
            chat.AppendUserInput(item);

            var response = await chat.GetResponseFromChatbotAsync();

            return response;

        }

        public async Task<string> GetYear(string brand, string serial)
        {
            OpenAIAPI openai = new OpenAIAPI(_key);

            var chat = openai.Chat.CreateConversation();

            if(brand == "Gibson")
            {
                chat.AppendSystemMessage("Using the following webpage as a reference, http://guitarhq.com/gibson.html#serial  . Please review the entire page as many serial numbers are ambiguous and there may be multiple possibilities. Your response format should only be an exact year such as '1997' or a decade such as '2000s'.");
                chat.AppendUserInput($"Serial Number: {serial}");
                return await chat.GetResponseFromChatbotAsync();

                
            }

            chat.AppendSystemMessage("You will be provided a musical instrument followed by the serial number. Please do your best to determine the year the instrument was manufactured with the information provided. Your response format should only be an exact year such as '1997' or list of possible years.");
            chat.AppendUserInput(brand);



            var response = await chat.GetResponseFromChatbotAsync();

            return response;

        }

    }
}




