using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WhatsAppKpiBot;

public class GptQuery
{
    private static string CallGpt(string question, string apikey, double temperature, int maxTokens, string model)
    {
        apikey = string.IsNullOrWhiteSpace(apikey) ? "sk-dummy" : apikey;
        Console.WriteLine($"effectve apikey: {apikey}");
        string result;
        var client = new RestClient("https://api.openai.com/");
        //var request = new RestRequest();
        var request = new RestRequest("v1/completions", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {apikey}");
        request.AddJsonBody(new
        {
            //model = "gpt-3.5-turbo",
            model,
            //prompt = "### Postgres SQL tables, with their properties:\n#\n# Employee(id, name, department_id)\n# Department(id, name, address)\n# Salary_Payments(id, employee_id, amount, date)\n#\n### A query to list the names of the departments which employed more than 10 employees in the last 3 months\nSELECT",
            prompt = question,
            temperature,
            //max_tokens = 400,
            max_tokens = maxTokens,
            top_p = 1.0,
            frequency_penalty = 0.0,
            presence_penalty = 0.0,
            stop = "['#', ';']"
        });

        //https://beta.openai.com/examples
        //var response = client.Execute(request);
        var response = client.Execute(request, Method.Post);
        var o = JObject.Parse(response.Content);
        //Json.ParseObject
        Debug.WriteLine(response.Content);
        result = o["choices"].First()["text"].ToString().Trim();
        return result;
    }

    public static Dictionary<string, string> AskQuestion(string question, string gptapikey)
    {
        var query = $@"basierend auf diesen query dimensionen und metriken

Dimensions:
city
country
product
campaignName

Metrics
totalRevenue
totalUsers
bounceRate
sessions

erzeuge ein JSON object in dieser form

{{
""Dimension"":""<DimensionName>"",
""Metric"":""<Metric>"",
""startDate"": ""<yyyy-MM-dd>"",
""endDate"": ""<yyyy-MM-dd>""
}}


auf basis dieser frage:
{question}? 

falls kein zeitraum angegeben ist, dann nimm die letzten 30 tage
";


        var Code = CallGpt(query, gptapikey, 0.5, 400, "gpt-3.5-turbo-instruct");
        Console.WriteLine("");
        Console.WriteLine(Code);
        Console.WriteLine("");
        var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(Code);
        return obj;
    }
}