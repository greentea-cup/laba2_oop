// vim: set fdm=indent :
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Diagnostics;

class Program {
	static int Main(string[] args) {
		HttpClient client = new HttpClient();
		HttpRequestHeaders headers = client.DefaultRequestHeaders;
		headers.Clear();
		headers.Add("Accept", "application/json");
		Uri[] urisToGet = {
			new Uri("https://httpstat.us/200"),
			new Uri("https://httpbin.org/json"),
			new Uri("https://raw.githubusercontent.com/mdn/browser-compat-data/refs/heads/main/package.json")
		};
		Stopwatch s2 = doAsyncRequests(client, urisToGet);
		Stopwatch s1 = doSyncRequests(client, urisToGet);
		Console.WriteLine("======");
		Console.WriteLine($"Sync  requests time: {s1.ElapsedMilliseconds}ms");
		Console.WriteLine($"Async requests time: {s2.ElapsedMilliseconds}ms");
		return 0;
	}

	static Stopwatch doSyncRequests(HttpClient client, Uri[] uris) {
		Stopwatch s = new Stopwatch();
		s.Start();
		for (int i = 0; i < uris.Length; i++) {
			try {
				Uri uri = uris[i];
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
				HttpResponseMessage response = client.Send(request);
				Stream stream = response.Content.ReadAsStream();
				StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
				string text = reader.ReadToEnd();
				Console.WriteLine($"Sync [{i}] response text:\n{text}");
			}
			catch {
				Console.WriteLine($"Sync [{i}] response failed due to unknown reason. Deal with it.");
			}
		}
		s.Stop();
		return s;
	}

	static Stopwatch doAsyncRequests(HttpClient client, Uri[] uris) {
		Stopwatch s = new Stopwatch();
		s.Start();
		var responses = new Task<HttpResponseMessage>[uris.Length];
		for (int i = 0; i < uris.Length; i++) {
			try {
				Uri uri = uris[i];
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
				responses[i] = client.SendAsync(request);
				Console.WriteLine($"Async [{i}] request successfully sent");
			}
			catch {
				Console.WriteLine($"Async [{i}] request failed to send due to unknown reason. Deal with it.");
			}
		}
		try {
			Task.WaitAll(responses);
		}
		catch {
			Console.WriteLine("Async some request failed to wait due to unknown reason. Deal with it.");
		}
		for (int i = 0; i < responses.Length; i++) {
			try {
				var response = responses[i].Result;
				Stream stream = response.Content.ReadAsStream();
				StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
				string text = reader.ReadToEnd();
				Console.WriteLine($"Async [{i}] response text:\n{text}");
			}
			catch {
				Console.WriteLine($"Async [{i}] response failed to read. Deal with it.");
			}
		}
		s.Stop();
		return s;
	}
}
