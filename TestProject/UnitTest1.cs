using WhatsAppKpiBot;
using WhatsAppKpiBot.Controllers;

namespace TestProject;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var foo = KpiBotController.GetGoogleAnalyticsMetrics("330445318", new Dictionary<string, string>
                {
                    {
                        "Metric", "sessions"
                    }
                })
                .ToArray()
            ;
    }

    [TestMethod]
    public void TestMethod2()
    {
        GptQuery.AskQuestion("wieviel Umsatz pro stadt", "sk-???")
            ;
    }


    [TestMethod]
    public void TestMethod3()
    {
        Environment.SetEnvironmentVariable("gptapikey", "sk-????");
        KpiBotController.GetGptAnswer("wieviel Umsatz pro kampagne")
            ;
    }
}