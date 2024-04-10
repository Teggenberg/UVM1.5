using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenAI_API;
using OpenAI_API.Completions;
using System;
using System.Net.Http.Headers;
using System.Text;
using UVM1._5.Models;
using Python.Runtime;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace UVM1._5.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class OpenAIController : ControllerBase
    {
        private string _key = "";
        static readonly HttpClient client = new HttpClient();

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

            chat.AppendSystemMessage("Using only one of the categories in the following list, " +
                "please tell me which categpry the provided item falls best into. " +
                "Please respond with Only a category selection included in the following list " +
                "and use the exact spelling as the list value.");
            chat.AppendSystemMessage($"The List contains the following: {categories[0]}");
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

        /*static async Task Main()
        {
            // Your OpenAI API key
            string apiKey = "your_api_key_here";
            // The image you want to analyze
            byte[] imageBytes = File.ReadAllBytes("path_to_your_image.jpg");
            // The description to match
            string description = "A description of the image";

            await MatchImageToDescriptionAsync(apiKey, imageBytes, description);
        }*/

        public async Task MatchImageToDescriptionAsync(byte[] imageBytes, string description)
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this._key);

                var requestData = new
                {
                    image = Convert.ToBase64String(imageBytes),
                    descriptions = new string[] { description }
                };

                string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                System.Diagnostics.Debug.WriteLine("\nException Caught!");
                System.Diagnostics.Debug.WriteLine("Message :{0} ", e.Message);
            }
        }

        public void GPTVision()
        {
            Runtime.PythonDLL = @"C:\Users\timeg\UVM1.5\UVM1.5\UVM1.5\python39.dll";
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                var pythonScript = Py.Import("PythonScript");
                pythonScript.InvokeMethod("say_hello");
            }






        }

        public async Task CheckImage(string imgUrl, string prompt)
        {
            string endPoint = "https://api.openai.com/v1/chat/completions";
            HttpClient client = new HttpClient();
            var request = new VisionRequest( imgUrl, prompt);

            try
            {
                /*client.BaseAddress = new Uri(endPoint);

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _key);

                client.DefaultRequestHeaders.Add("Accept", "application/json");

                client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                
*/
                string json = JsonSerializer.Serialize(request);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");

                var response = await client.PostAsync(endPoint, requestContent);
                string responseBody = await response.Content.ReadAsStringAsync();
                VisionResponse vResponse = new VisionResponse();
                vResponse = JsonSerializer.Deserialize<VisionResponse>(responseBody);
                int i = 0;

                string answer = vResponse.choices[0].message.content;



            }
            catch(Exception ex)
            {
                var message = ex.Message;
                var extest = "";

            }
            

            var test = "";



        }  

    }
}




