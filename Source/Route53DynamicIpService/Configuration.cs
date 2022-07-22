using Amazon.Runtime;

namespace Route53DynamicIpUpdater
{
  public class Configuration
  {
    public TimeSpan Interval { get; }
    public string HostedZoneId { get; }
    public AWSCredentials AWSCredentials { get; }
    public List<string> Names { get; }

    public Configuration(IConfiguration options) : this(options.GetSection("Configuration")) { }
    public Configuration(IConfigurationSection options)
    {
      Interval = TimeSpan.Parse(options["Interval"]);
      HostedZoneId = options["Hosted Zone ID"];
      var aws = options.GetRequiredSection("AWS Credentials");
      AWSCredentials = new BasicAWSCredentials(aws["Access Key"], aws["Secret Key"]);
      Names = options.GetRequiredSection("Names").GetChildren().Select(x=>x.Get<String>()).ToList();
    }
  }
}
