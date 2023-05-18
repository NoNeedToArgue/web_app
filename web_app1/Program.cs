using Lib;
using System.Text;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/getusers", async (context) =>
{
    app.Logger.LogInformation($"Path: /getusers  Time: {DateTime.Now.ToLongTimeString()}");
    if (context.Request.QueryString.ToString().Count() > 0)
        app.Logger.LogInformation($"Processing request {context.Request.QueryString}  Time: {DateTime.Now.ToLongTimeString()}");

    string? name = null;
    string? city = null;

    foreach (var param in context.Request.Query)
    {
        if (param.Key.ToLower() == "name")
        {
            name = param.Value;
        }
        if (param.Key.ToLower() == "city")
        {
            city = param.Value;
        }
    }

    var stringBuilder = new StringBuilder();

    using (ApplicationContext db = new())
    {
        var users = db.Users.Where(u => (name != null ? u.Name == name : true) && (city != null ? u.City == city : true));
           
        foreach (User u in users)
        {
            stringBuilder.Append($"{u.Id}. {u.Name} - {u.City}\n");
        }
    }

    await context.Response.WriteAsync(stringBuilder.ToString());
});

app.MapGet("/producer", (ILogger<Program> logger) =>
{
    string messageInfo = Producer.Send();
    app.Logger.LogInformation($"Path: /producer  Time: {DateTime.Now.ToLongTimeString()}\n{messageInfo}");
    return messageInfo;
});

app.MapGet("/consumer", (ILogger<Program> logger) =>
{
    string messageInfo = Consumer.Receive();
    app.Logger.LogInformation($"Path: /consumer  Time: {DateTime.Now.ToLongTimeString()}\n{messageInfo}");
    return messageInfo;
});

app.MapGet("/", async (context) =>
{
    app.Logger.LogInformation($"Processing request {context.Request.QueryString}  Time: {DateTime.Now.ToLongTimeString()}");
    
    context.Response.ContentType = "text/html; charset=utf-8";
    var stringBuilder = new StringBuilder("<h3>Request</h3><table>");
    stringBuilder.Append("<tr><td>Parameter</td><td>Value</td></tr>");
    string? username = null;
    string? city = null;
    foreach (var param in context.Request.Query)
    {
        if (param.Key.ToLower() == "name")
        {
            username = param.Value;
        }
        if (param.Key.ToLower() == "city")
        {
            city = param.Value;
        }
        stringBuilder.Append($"<tr><td>{param.Key}</td><td>{param.Value}</td></tr>");
    }
    if (username != null && city != null)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            db.Users.Add(new User { Name = username, City = city });
            db.SaveChanges();
        } 
    }
    stringBuilder.Append("</table>");
    await context.Response.WriteAsync(stringBuilder.ToString());
});

app.UseMetricServer();

app.Run();
