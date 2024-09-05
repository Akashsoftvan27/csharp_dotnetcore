using Newtonsoft.Json.Linq;

public static class AdaptiveCardGenerator
{
    public static JObject CreatePermitAdaptiveCard(string permitNumber, string permitType, string permitStatus, string sPDFURL)
    {
        var adaptiveCardJson = new JObject
        {
            ["type"] = "AdaptiveCard",
            ["body"] = new JArray
            {
                new JObject
                {
                    ["type"] = "TextBlock",
                    ["text"] = $"Permit Number: {permitNumber}",
                    ["weight"] = "Bolder",
                    ["size"] = "Medium" 
                },
                new JObject
                {
                    ["type"] = "TextBlock",
                    ["text"] = $"Permit Type: {permitType}"
                },
                new JObject
                {
                    ["type"] = "TextBlock",
                    ["text"] = $"Status: {permitStatus}"
                },
                 new JObject
                {
                    ["type"] = "TextBlock",
                    ["text"] = $"[View PDF/Download PDF]({sPDFURL})"
                },
              
            },
            ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json",
            ["version"] = "1.2"
        };

        return adaptiveCardJson;
    }
}
