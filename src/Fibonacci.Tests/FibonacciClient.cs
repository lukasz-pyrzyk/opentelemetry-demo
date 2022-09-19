using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fibonacci.Tests;

public class FibonacciClient
{
    private readonly HttpClient _client = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:5001")
    };

    public async Task<int> Calculate(int n)
    {
        var location = await PostRequest(n);
        return await ReadResult(location);
    }

    private async Task<Uri> PostRequest(int n)
    {
        var getActivity = new Activity("post call from client")
            .AddBaggage("sender", "client")
            .Start();

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/{n}");
        Uri location = null;
        try
        {
            using var postResponse = await _client.SendAsync(postRequest);
            postResponse.EnsureSuccessStatusCode();
            location = postResponse.Headers.Location;
        }
        finally
        {
            getActivity.Stop();
        }

        return location;
    }

    private async Task<int> ReadResult(Uri location)
    {
        await Task.Delay(TimeSpan.FromSeconds(5)); // wait for backend

        var postActivity = new Activity("post call from client")
            .AddBaggage("sender", "client")
            .Start();

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, location);
        try
        {
            using var getResponse = await _client.SendAsync(getRequest);
            getResponse.EnsureSuccessStatusCode();

            var calculatedValue = await getResponse.Content.ReadAsStringAsync();
            return int.Parse(calculatedValue);
        }
        finally
        {
            postActivity.Stop();
        }
    }


    public async Task DeleteResult(int n)
    {
        var activity = new Activity("call from client")
            .AddBaggage("sender", "client")
            .Start();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/{n}");
            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            activity.Stop();
        }
    }
}