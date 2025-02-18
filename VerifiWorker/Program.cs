using Coravel;
using Microsoft.Extensions.Configuration;
using Serilog;
using VerifiWorker;


try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.Configure<ArcherConnectorOptions>(
        builder.Configuration.GetSection(
            key: nameof(ArcherConnectorOptions)));
    builder.Services.Configure<VerifiConnectorOptions>(
        builder.Configuration.GetSection(
            key: nameof(VerifiConnectorOptions)));
    builder.Services.Configure<VerifiConnectorLegacyOptions>(
        builder.Configuration.GetSection(
            key: nameof(VerifiConnectorLegacyOptions)));
    builder.Services.AddSerilog((config) =>
    {
        config.ReadFrom.Configuration(builder.Configuration);
    });
    builder.Services.AddTransient<ArcherConnector>();
    builder.Services.AddTransient<VerifiConnector>();
    builder.Services.AddTransient<VerifiConnectorLegacy>();
    builder.Services.AddSingleton<TicketTracker>();

    builder.Services.AddScheduler();
    builder.Services.Configure<ProcessorOptions>(
        builder.Configuration.GetSection(
            key: nameof(ProcessorOptions)));
    builder.Services.AddTransient<Processor>();
    builder.Services.AddWindowsService();

    var host = builder.Build();

    host.Services.UseScheduler(scheduler =>
    {
        scheduler.Schedule<Processor>()
        .EveryFifteenSeconds()
        .PreventOverlapping(nameof(Processor));
    });

    host.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Service has shutdown unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

