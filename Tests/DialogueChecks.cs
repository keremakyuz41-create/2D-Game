using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cernobil.Dialogue;

// Standalone checks use the actual production policy and HTTP client sources.
// An in-memory mock transport is not an LLM; these checks do not establish dialogue quality.
internal static class DialogueChecks
{
    private static int passed;
    private static void Check(bool result, string label)
    {
        if (!result) throw new Exception(label);
        passed++;
        Console.WriteLine("PASS " + label);
    }
    private static NpcReply Reply(string action = "none", string target = "none", string text = "Merhaba")
        => new NpcReply { reply = text, action = action, target = target };

    public static async Task Main()
    {
        Check(DialoguePolicy.Validate(Reply(), false, false, out _), "Sohbet kapıdan bağımsız");
        Check(DialoguePolicy.Validate(Reply("assign_mission", "evacuation"), true, false, out _), "Görev koşulları uygun");
        Check(!DialoguePolicy.Validate(Reply("assign_mission", "evacuation"), false, false, out _), "Kapalı kapıda görev reddi");
        Check(!DialoguePolicy.Validate(Reply("assign_mission", "evacuation"), true, true, out _), "Görev tekrarı reddi");
        Check(!DialoguePolicy.Validate(Reply("open_gate", "gate"), true, false, out _), "Robot kapısını atlama reddi");
        Check(!DialoguePolicy.Validate(Reply("give_ammo", "rifle"), true, false, out _), "Cephane komutu reddi");
        Check(!DialoguePolicy.Validate(Reply("assign_mission", "other"), true, false, out _), "Yanlış hedef reddi");
        Check(!DialoguePolicy.Validate(Reply("none", "evacuation"), true, false, out _), "Tutarsız none hedefi reddi");
        Check(!DialoguePolicy.Validate(Reply(null, null), true, false, out _), "Eksik alan reddi");
        Check(!DialoguePolicy.Validate(Reply(text: " "), true, false, out _), "Boş metin reddi");
        Check(!DialoguePolicy.Validate(Reply(text: new string('x', 701)), true, false, out _), "Uzun metin reddi");
        Check(!DialoguePolicy.Validate(null, true, false, out _), "Null yanıt reddi");
        Check(ShelterAccessPolicy.Validate(Reply(), true, false, out _), "Görevli serbest sohbet");
        Check(ShelterAccessPolicy.Validate(Reply("grant_access", "shelter"), true, false, out _), "Sığınak giriş izni");
        Check(!ShelterAccessPolicy.Validate(Reply("grant_access", "shelter"), false, false, out _), "Uzaktan giriş izni reddi");
        Check(!ShelterAccessPolicy.Validate(Reply("grant_access", "shelter"), true, true, out _), "Tekrarlanan giriş izni reddi");
        Check(!ShelterAccessPolicy.Validate(Reply("open_gate", "santral"), true, false, out _), "Yanlış kapı komutu reddi");
        Check(!ShelterAccessPolicy.Validate(Reply("give_ammo", "rifle"), true, false, out _), "Görevli cephane veremez");
        Check(!ShelterAccessPolicy.Validate(Reply("grant_access", "other"), true, false, out _), "Girişte yanlış hedef reddi");
        Check(!ShelterAccessPolicy.Validate(Reply(null, null), true, false, out _), "Görevli eksik JSON alan reddi");
        Check(!ShelterAccessPolicy.Validate(null, true, false, out _), "Görevli null reddi");
        Check(!ShelterAccessPolicy.Validate(Reply(text: " "), true, false, out _), "Görevli boş yanıt reddi");
        int policyChecks = passed;
        await Transport(200, "{\"done\":true}", null);
        await Transport(404, "model not found", "Model bulunamadı");
        await Transport(500, "failure", "HTTP 500");
        await Transport(302, "", "HTTP 302");
        using (var client = new OllamaClient(new FakeHandler { Offline = true }))
        {
            try { await client.ChatAsync("{}", CancellationToken.None); Check(false, "Sunucu kapalıyken hata bekleniyor"); }
            catch (InvalidOperationException ex) { Check(ex.Message.Contains("Yerel sunucuya"), "Sunucu kapalı hata mesajı"); }
        }
        using (var client = new OllamaClient(new FakeHandler { Delay = true }))
        using (var cancel = new CancellationTokenSource())
        {
            Task<string> request = client.ChatAsync("{}", cancel.Token);
            cancel.Cancel();
            try { await request; Check(false, "İptal bekleniyor"); }
            catch (OperationCanceledException) { Check(true, "Devam eden istek iptali"); }
        }
        File.WriteAllText("NPC-Standalone-Test-Sonuclari.json", "{\"status\":\"passed\",\"policy_checks\":" + policyChecks +
            ",\"mock_transport_checks\":" + (passed - policyChecks) + ",\"live_llm_tested\":false,\"unity_ui_tested\":false}");
        Console.WriteLine("TOTAL " + passed);
    }

    private static async Task Transport(int status, string body, string expectedError)
    {
        using (var client = new OllamaClient(new FakeHandler { Status = status, Body = body }))
        {
            try
            {
                string result = await client.ChatAsync("{\"model\":\"test-local\"}", CancellationToken.None);
                Check(expectedError == null && result == body, "HTTP " + status);
            }
            catch (InvalidOperationException ex)
            { Check(expectedError != null && ex.Message.Contains(expectedError), "HTTP " + status + " kontrollü hata"); }
        }
    }

    private sealed class FakeHandler : System.Net.Http.HttpMessageHandler
    {
        public int Status = 200;
        public string Body = "";
        public bool Offline;
        public bool Delay;
        protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellation)
        {
            if (request.RequestUri.ToString() != "http://127.0.0.1:11434/api/chat" || request.Method != System.Net.Http.HttpMethod.Post)
                throw new Exception("Endpoint must remain local POST /api/chat");
            if (request.Content.Headers.ContentType.MediaType != "application/json") throw new Exception("Content-Type");
            if (Offline) throw new System.Net.Http.HttpRequestException("Simulated server unavailable");
            if (Delay) await Task.Delay(10000, cancellation);
            return new System.Net.Http.HttpResponseMessage((HttpStatusCode)Status) { Content = new System.Net.Http.StringContent(Body) };
        }
    }
}
