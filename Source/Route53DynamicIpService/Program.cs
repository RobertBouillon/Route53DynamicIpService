using Microsoft.Extensions.Logging.EventLog;

namespace Route53DynamicIpUpdater
{
  public class Program
  {
    public static void Main(string[] args)
    {
      IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.secrets.json"))
        .ConfigureLogging(logging => logging.AddEventLog(settings => settings.SourceName = "Route 53 Dynamic IP"))
        .ConfigureServices((context, services) =>
        {
          services.AddSingleton(new Configuration(context.Configuration));
          services.AddHostedService<Worker>();
        })
        .UseWindowsService()
        .Build();

      host.Run();
    }
  }
}