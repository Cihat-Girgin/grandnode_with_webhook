using Grand.Web.Common.Extensions;
using Grand.Web.Common.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using StartupBase = Grand.Infrastructure.StartupBase;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((_, options) =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()  
    .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs", "log.txt"), rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();

builder.Logging.AddSerilog();

//add configuration
builder.Configuration.AddAppSettingsJsonFile(args);

//add services
StartupBase.ConfigureServices(builder.Services, builder.Configuration);

builder.ConfigureApplicationSettings();

//register task
builder.Services.RegisterTasks(builder.Configuration);

//build app
var app = builder.Build();

//request pipeline
StartupBase.ConfigureRequestPipeline(app, builder.Environment);

//run app
app.Run();