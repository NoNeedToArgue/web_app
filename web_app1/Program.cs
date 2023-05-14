using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/reset", (ILogger<Program> logger) =>
{
    logger.LogInformation($"Path: /reset  Time: {DateTime.Now.ToLongTimeString()}");
    Latency.ResetLatency();
    return "Application reset";
});

app.MapGet("/data", async (ILogger<Program> logger) =>
{
    logger.LogInformation($"Path: /data  Time: {DateTime.Now.ToLongTimeString()}");
    int latency = Latency.GetLatency();
    await Task.Delay(latency);
    return $"Application latency: {latency}";
});

app.MapGet("/", async (context) =>
{
    app.Logger.LogInformation($"Processing request {context.Request.QueryString}  Time: {DateTime.Now.ToLongTimeString()}");
    context.Response.ContentType = "text/html; charset=utf-8";
    var stringBuilder = new System.Text.StringBuilder("<h3>Request</h3><table>");
    stringBuilder.Append("<tr><td>Parameter</td><td>Value</td></tr>");
    foreach (var param in context.Request.Query)
    {
        stringBuilder.Append($"<tr><td>{param.Key}</td><td>{param.Value}</td></tr>");
    }
    stringBuilder.Append("</table>");
    await context.Response.WriteAsync(stringBuilder.ToString());
});

app.UseMetricServer();

app.Run();

static class Latency
{
    static int counter = 1;
    public static int GetLatency() => counter++ * 500;
    public static void ResetLatency() => counter = 1;
}