using System.Text;
using System.Text.Json;
using Google.Analytics.Data.V1Beta;
using Microsoft.AspNetCore.Mvc;

namespace WhatsAppKpiBot.Controllers;

[ApiController]
[Route("[controller]")]
public class KpiBotController : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> MyHttpPostMethod()
    {
        // Get the HTTP request
        var request = HttpContext.Request;

        // Read the request body
        using (var reader = new StreamReader(request.Body))
        {
            var requestBody = await reader.ReadToEndAsync();
            Console.WriteLine(requestBody);
            try
            {
                var receivedMessage = JsonSerializer.Deserialize<RootObject>(requestBody);
                var query = receivedMessage.entry[0].changes[0].value.messages[0].text.body;

                var answer = GetGptAnswer(query);
                await SendToWhatsApp(answer, receivedMessage.entry[0].changes[0].value.messages[0].from);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return Content("POST request processed successfully");
        }
    }

    public static string GetGptAnswer(string query)
    {
        var gptApiKey = Environment.GetEnvironmentVariable("gptapikey");
        var args = GptQuery.AskQuestion(query, gptApiKey);

        var answer =
            string.Join("\r\n\r\n", GetGoogleAnalyticsMetrics("330445318", args).Take(10).Select(r => r.ToString()));
        return answer;
    }


    public void AskGpt()
    {
        //ask chatgpt api
    }


    [HttpGet("webhook")]
    public IActionResult GetWebhook([FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        /**
         * UPDATE YOUR VERIFY TOKEN
         * This will be the Verify Token value when you set up webhook
         **/
        var verifyToken = "angry-roomy-pair";

        // Check if a token and mode were sent
        if (mode != null && token != null)
        {
            // Check the mode and token sent are correct
            if (mode == "subscribe" && token == verifyToken)
            {
                // Respond with 200 OK and challenge token from the request
                Console.WriteLine("WEBHOOK_VERIFIED");
                return Ok(challenge);
            }

            // Responds with '403 Forbidden' if verify tokens do not match
            return StatusCode(403);
        }

        return BadRequest("Invalid request");
    }

    /// <summary>
    ///     Do a Meta API call and send messages to WhatsApp
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static async Task SendToWhatsApp(string messageText, string to)
    {
        //please edit file  WhatsAppKpiBot/Properties/launchSettings.json and add your own values for token and phone number id
        var phoneid = Environment.GetEnvironmentVariable("whatsappphoneid");
        var url = $"https://graph.facebook.com/v18.0/{phoneid}/messages";
        var accessToken = Environment.GetEnvironmentVariable("whatsapptoken");

        var client = new HttpClient();

        var requestData = new
        {
            messaging_product = "whatsapp",
            to,
            type = "text",
            text = new
            {
                body = messageText
            }
        };

        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

        var response = await client.PostAsync(url, content);

        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine(response.StatusCode);
        Console.WriteLine(responseBody);
    }

    /// <summary>
    ///     Check https://ga-dev-tools.google/ga4/query-explorer/
    ///     https://developers.google.com/analytics/devguides/reporting/data/v1/api-schema#metrics
    /// </summary>
    /// <param name="propertyId"></param>
    /// <returns></returns>
    public static IEnumerable<ReportDataRow> GetGoogleAnalyticsMetrics(string propertyId, Dictionary<string, string> args)
    {
        string dimension, metric, startDate, endDate;

        if (args.TryGetValue("Dimension", out dimension))
        {
            //do something
        }

        args.TryGetValue("Metric", out metric);

        if (!args.TryGetValue("startDate", out startDate))
        {
            startDate = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
        }

        if (!args.TryGetValue("endDate", out endDate))
        {
            endDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        }

        if (startDate.Equals("<yyyy-MM-dd>"))
        {
            startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            endDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        }


        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "/app/data/credentials.json");
        var client = new BetaAnalyticsDataClientBuilder().Build();

        RunReportRequest request = null;

        if (dimension == null)
            request = new RunReportRequest
            {
                Property = "properties/" + propertyId,
                //Dimensions =
                //{
                //    new Dimension
                //    {
                //        Name = dimension
                //    }
                //},
                Metrics =
                {
                    new Metric
                    {
                        Name = metric
                    }
                },
                DateRanges =
                {
                    new DateRange
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }
                }
            };

        else
            request = new RunReportRequest
            {
                Property = "properties/" + propertyId,
                Dimensions =
                {
                    new Dimension
                    {
                        Name = dimension
                    }
                },
                Metrics =
                {
                    new Metric
                    {
                        Name = metric
                    }
                },
                DateRanges =
                {
                    new DateRange
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }
                }
            };
        //return client.RunReport(request).Rows.Select(row => $"Campaign: {row.DimensionValues[0].Value} - Revenue: {row.MetricValues[0].Value} - Cost: {row.MetricValues[1].Value}");

        client.RunReport(request);
        // Assuming client.RunReport(request) returns IEnumerable<Row> or similar collection
        var reportData = client.RunReport(request).Rows.Select(row =>
                new ReportDataRow
                {
                    Campaign = row.DimensionValues.Count > 0 ? row.DimensionValues[0].Value : null,
                    Revenue = double.Parse(row.MetricValues[0].Value)
                })
            .ToList();


        return reportData;
    }

    public class ReportDataRow
    {
        public string Campaign { get; set; }
        public double Revenue { get; set; }
        public double Cost { get; set; }

        public double Roas => Cost != 0d ? Revenue / Cost : 0;

        public override string ToString()
        {
            var warnung = string.Empty;
            if (Roas == 0)
            {
            }
            else if (Roas < 1)
            {
                warnung = "\ud83d\udfe5";
            }
            else if (Roas < 4)
            {
                warnung = "\ud83d\udfe8";
            }

            //var ci = new CultureInfo("de-DE");
            return $"{TruncateString(Campaign, 8)} -  {Revenue.ToString("F")} ";
        }

        public static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}