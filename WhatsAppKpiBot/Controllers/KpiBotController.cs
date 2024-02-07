using System.Text;
using System.Text.Json;
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
                await SendToWhatsApp(receivedMessage.entry[0].changes[0].value.messages[0].text.body, receivedMessage.entry[0].changes[0].value.messages[0].from);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Content("POST request processed successfully");
        }
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
    /// Do a Meta API call and send messages to WhatsApp
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
}