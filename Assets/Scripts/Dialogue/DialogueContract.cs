using System;

namespace Cernobil.Dialogue
{
    [Serializable]
    public sealed class NpcReply
    {
        public string reply;
        public string action;
        public string target;
    }

    // The model proposes an action. Only game state may authorize it.
    public static class DialoguePolicy
    {
        public static bool Validate(NpcReply value, bool gateOpen, bool missionAssigned, out string error)
        {
            error = null;
            if (value == null || string.IsNullOrWhiteSpace(value.reply) || value.reply.Length > 700)
                error = "NPC yanıtı eksik veya çok uzun. Yeniden deneyebilirsin.";
            else if (value.action == "none" && value.target == "none") return true;
            else if (value.action != "assign_mission" || value.target != "evacuation")
                error = "NPC desteklenmeyen bir işlem istedi. İşlem uygulanmadı.";
            else if (!gateOpen) error = "Tahliye görevi için önce robot santral kapısını açmalı.";
            else if (missionAssigned) error = "Tahliye görevi zaten alındı; tekrar uygulanmadı.";
            return error == null;
        }
    }
}
