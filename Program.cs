var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/{switchName}/{status:regex(^on+$|^off+$)}/{securityCode}", (string switchName, string status, string securityCode) => TurnOnSwitchAsync(switchName, status, securityCode));
app.Run("http://*:5012");

async Task<IResult> TurnOnSwitchAsync(string switchName, string status, string securityCode)
{
    string HlGuid = Environment.GetEnvironmentVariable("HL_GUID") ?? throw new Exception("HL_GUID environment variable not set");
    if (securityCode != HlGuid)
    {
        return Results.BadRequest("Invalid security code");
    }

    string HaAPI = Environment.GetEnvironmentVariable("HA_API") ?? throw new Exception("HA_API environment variable not set");
    string HaURLBase = Environment.GetEnvironmentVariable("HA_URL") ?? throw new Exception("HA_URL environment variable not set");
    string HaUrl = $"{HaURLBase}/api/services/switch/turn_{status}";
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HaAPI);
    var result = await httpClient.PostAsJsonAsync(HaUrl, new { entity_id = $"switch.{switchName}" });
    result.EnsureSuccessStatusCode();
    return Results.Ok($"Switch turned {status}");
}