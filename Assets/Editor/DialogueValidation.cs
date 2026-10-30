using System;
using System.IO;
using Cernobil.Dialogue;
using UnityEditor;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class DialogueValidation
    {
        [MenuItem("Cernobil/NPC - JSON ve Gorev Kontrolleri")]
        public static void Run()
        {
            int count = 0;
            Check("Normal sohbet", new NpcReply { reply = "Merhaba", action = "none", target = "none" }, false, false, true, ref count);
            Check("Görev ver", new NpcReply { reply = "Bölgeyi temizle", action = "assign_mission", target = "evacuation" }, true, false, true, ref count);
            Check("Kapı kapalı", new NpcReply { reply = "Görev", action = "assign_mission", target = "evacuation" }, false, false, false, ref count);
            Check("Tekrarlanan görev", new NpcReply { reply = "Görev", action = "assign_mission", target = "evacuation" }, true, true, false, ref count);
            Check("Yetkisiz kapı aç", new NpcReply { reply = "Açtım", action = "open_gate", target = "gate" }, true, false, false, ref count);
            Check("Yanlış hedef", new NpcReply { reply = "Görev", action = "assign_mission", target = "rifle" }, true, false, false, ref count);
            Check("Boş yanıt", new NpcReply { reply = "", action = "none", target = "none" }, true, false, false, ref count);
            Check("Eksik alan", JsonUtility.FromJson<NpcReply>("{\"reply\":\"Merhaba\"}"), true, false, false, ref count);
            Check("Uzun yanıt", new NpcReply { reply = new string('a', 701), action = "none", target = "none" }, true, false, false, ref count);
            Check("Null", null, true, false, false, ref count);
            File.WriteAllText("NPC-Politika-Test-Sonuclari.json", "{\"status\":\"passed\",\"checks\":" + count + ",\"scope\":\"JSON/action policy only; not live model dialogue\"}");
            Debug.Log("NPC işlem politikası: " + count + "/" + count + ". Canlı Ollama konuşmaları ayrıca test edilmeli.");
        }

        private static void Check(string name, NpcReply reply, bool gate, bool assigned, bool expected, ref int count)
        {
            if (DialoguePolicy.Validate(reply, gate, assigned, out _) != expected) throw new InvalidOperationException("NPC testi başarısız: " + name);
            count++;
        }
    }
}
