namespace Route53DynamicIpUpdater;

internal class Ipify : IDisposable
{
  private HttpClient _client;

  public void Dispose() => _client.Dispose();

  public Ipify() => _client = new HttpClient();

  public async Task<string> FetchPublicIpAsync()
  {
    HttpResponseMessage response = null;
    while (true)
    {
      try
      {
        response = await _client.GetAsync("https://api.ipify.org");
      }
      catch { }

      if (response?.IsSuccessStatusCode ?? false)
        break;
      else
        await Task.Delay(TimeSpan.FromSeconds(10));
    }

    return await response.Content.ReadAsStringAsync();
  }
}
