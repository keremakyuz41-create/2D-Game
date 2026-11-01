using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cernobil.Dialogue
{
    public interface INpcLanguageModel
    {
        Task<string> ChatAsync(string json, CancellationToken cancellation);
    }

    // Fixed loopback endpoint, no proxy, redirects or cloud fallback.
    public sealed class OllamaClient : INpcLanguageModel, IDisposable
    {
        private readonly HttpClient http;
        public OllamaClient() : this(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false }) { }

        // In-memory transport injection for deterministic error/cancellation tests.
        internal OllamaClient(HttpMessageHandler handler)
        {
            http = new HttpClient(handler)
            { Timeout = TimeSpan.FromSeconds(90), MaxResponseContentBufferSize = 65536 };
        }

        public async Task<string> ChatAsync(string json, CancellationToken cancellation)
        {
            try
            {
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await http.PostAsync("http://127.0.0.1:11434/api/chat", content, cancellation).ConfigureAwait(false))
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        throw new InvalidOperationException("Model bulunamadı. Terminalde: ollama pull qwen2.5:3b");
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("Ollama yanıt veremedi (HTTP " + (int)response.StatusCode + "). Tekrar deneyebilirsin.");
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                if (cancellation.IsCancellationRequested) throw;
                throw new InvalidOperationException("Yanıt 90 saniyede gelmedi. Model yükleniyor olabilir; tekrar deneyebilirsin.");
            }
            catch (HttpRequestException)
            {
                throw new InvalidOperationException("Yerel sunucuya ulaşılamadı. Ollama uygulamasını açıp tekrar dene.");
            }
        }

        public void Dispose() => http.Dispose();
    }
}
