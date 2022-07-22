using System.Net;

namespace Route53DynamicIpUpdater;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private Configuration Configuration { get; }
  private Route53Client _r53;
  private Ipify _ipify;
  private string _ip;

  public Worker(ILogger<Worker> logger, Configuration configuration)
  {
    _logger = logger;
    Configuration = configuration;
    _ipify = new Ipify();
    _r53 = new Route53Client(configuration.AWSCredentials);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _ip = await GetCurrentIpAsync();
    _logger.LogInformation("{name} is {ip}", Configuration.Names.First(), _ip);
    while (!stoppingToken.IsCancellationRequested)
    {
      var ip = await _ipify.FetchPublicIpAsync();
      if (ip != _ip)
      {
        _logger.LogInformation("{name} changed to {ip}", Configuration.Names.First(), ip);
        await _r53.UpdateAddresses(Configuration.HostedZoneId, Configuration.Names, ip);
        _ip = ip;
      }
      await Task.Delay(Configuration.Interval, stoppingToken);
    }
  }

  private async Task<string> GetCurrentIpAsync()
  {
    var response = await Dns.GetHostEntryAsync(Configuration.Names.First());
    return response.AddressList.Any() ?
      response.AddressList.First().ToString() :
      String.Empty;
  }

  public override void Dispose()
  {
    _r53?.Dispose();
    _ipify?.Dispose();
    base.Dispose();
  }
}