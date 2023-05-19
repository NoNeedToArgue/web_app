using Lib;
using System.Text;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<UserService>();

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = "redis:6379";
    options.InstanceName = "test";
});

var app = builder.Build();

app.MapGet("/user/{id}", async (int id, UserService userService) =>
{
    app.Logger.LogInformation($"Path: /user/{id}  Time: {DateTime.Now.ToLongTimeString()}");

    string response = "User not found\nTwo second database access latency was simulated";

    User? user = await userService.GetUser(id);
    if (user != null)
    {
        response = $"User received from cache\n\nUser {user.Name}  Id = {user.Id}  City = {user.City}";
    }
    else
    {
        Thread.Sleep(2000);

        using (ApplicationContext db = new())
        {
            user = db.Users.Where(u => u.Id == id).SingleOrDefault();
        }
        if (user != null)
        {
            userService.SetUser(user);
            response = $"User received from db\nTwo second latency was simulated\n\nUser {user.Name}  Id = {user.Id}  City = {user.City}";
        }
    }
    
    app.Logger.LogInformation(response);

    return response;
});

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

    string response = stringBuilder.ToString().Length > 0 ? stringBuilder.ToString().Trim() : "No records";

    app.Logger.LogInformation(response);

    await context.Response.WriteAsync(response);
});

app.MapGet("/producer", () =>
{
    app.Logger.LogInformation($"Path: /producer  Time: {DateTime.Now.ToLongTimeString()}");
    
    string messageInfo = Producer.Send();
    
    app.Logger.LogInformation(messageInfo);

    return messageInfo;
});

app.MapGet("/consumer", () =>
{
    app.Logger.LogInformation($"Path: /consumer  Time: {DateTime.Now.ToLongTimeString()}");

    string messageInfo = Consumer.Receive();

    app.Logger.LogInformation(messageInfo);

    return messageInfo;
});

app.MapGet("/", async (context) =>
{
    app.Logger.LogInformation($"Path: /  Time: {DateTime.Now.ToLongTimeString()}");
    if (context.Request.QueryString.ToString().Count() > 0)
        app.Logger.LogInformation($"Processing request {context.Request.QueryString}  Time: {DateTime.Now.ToLongTimeString()}");

    context.Response.ContentType = "text/html; charset=utf-8";

    var stringBuilder = new StringBuilder("<h3>Request</h3><table>");
    stringBuilder.Append("<tr><td>Parameter</td><td>Value</td></tr>");
    
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

        stringBuilder.Append($"<tr><td>{param.Key}</td><td>{param.Value}</td></tr>");
    }

    if (name != null && city != null)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            db.Users.Add(new User { Name = name, City = city });
            db.SaveChanges();
        } 
    }

    stringBuilder.Append("</table>");
    
    await context.Response.WriteAsync(stringBuilder.ToString());
});

app.UseMetricServer();

app.Run();
