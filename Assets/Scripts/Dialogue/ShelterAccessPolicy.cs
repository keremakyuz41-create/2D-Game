namespace Cernobil.Dialogue
{
    public static class ShelterAccessPolicy
    {
        public static bool Validate(NpcReply value, bool nearby, bool authorized, out string error)
        {
            error = null;
            if (value == null || string.IsNullOrWhiteSpace(value.reply) || value.reply.Length > 700)
                error = "Görevlinin yanıtı eksik veya çok uzun. Tekrar deneyebilirsin.";
            else if (value.action == "none" && value.target == "none") return true;
            else if (value.action != "grant_access" || value.target != "shelter")
                error = "Görevli geçersiz bir işlem istedi. Kapı açılmadı.";
            else if (!nearby) error = "İzin almak için görevlinin yanında olmalısın.";
            else if (authorized) error = "Giriş iznin zaten var. Açılan kapıdan ilerle.";
            return error == null;
        }
    }
}
