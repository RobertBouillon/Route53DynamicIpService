using Amazon;
using Amazon.Route53;
using Amazon.Route53.Model;
using Amazon.Runtime;

namespace Route53DynamicIpUpdater
{
  internal class Route53Client : IDisposable
  {
    private AmazonRoute53Client _client;

    public Route53Client(AWSCredentials credentials) => _client = new AmazonRoute53Client(credentials, RegionEndpoint.USEast1);

    public async Task UpdateAddresses(string zone, IEnumerable<string> names, string ip)
    {
      var batch = new ChangeBatch();
      batch.Changes.AddRange(
        CreateRecordSets(names, ip)
        .Select(x => new Change("UPSERT", x)
      ));

      await _client.ChangeResourceRecordSetsAsync(new ChangeResourceRecordSetsRequest(zone, batch));
    }

    private static IEnumerable<ResourceRecordSet> CreateRecordSets(IEnumerable<string> names, string ip)
    {
      foreach (var name in names)
      {
        var recordset = new ResourceRecordSet(name, RRType.A) { TTL = 300 };
        recordset.ResourceRecords.Add(new ResourceRecord(ip));
        yield return recordset;
      }
    }

    public void Dispose() => _client?.Dispose();
  }
}
