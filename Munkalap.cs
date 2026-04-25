namespace PCSherviz.Models;

/// <summary>
/// A munkalap lehetséges állapotai.
/// </summary>
public enum Statusz
{
    Beerkezett = 0,
    Diagnosztika = 1,
    Javitas = 2,
    Kesz = 3,
    Kiadva = 4
}

/// <summary>
/// Egy szerviz-munkát leíró munkalap.
/// </summary>
public class Munkalap
{
    // ── Azonosítók ──────────────────────────────────────────
    public int Id { get; set; }

    // ── Ügyfél ──────────────────────────────────────────────
    public string UgyfelNev { get; set; } = string.Empty;
    public string UgyfelTelefon { get; set; } = string.Empty;

    // ── Eszköz ──────────────────────────────────────────────
    public string EszkozTipus { get; set; } = string.Empty;   // pl. "Laptop", "Asztali PC"
    public string EszkozMarka { get; set; } = string.Empty;   // pl. "Lenovo ThinkPad X1"
    public string SorozatSzam { get; set; } = string.Empty;

    // ── Hiba ────────────────────────────────────────────────
    public string HibaLeiras { get; set; } = string.Empty;
    public string? TechnikusMegjegyzes { get; set; }

    // ── Árak ────────────────────────────────────────────────
    public List<Alkatresz> Alkatreszek { get; set; } = new();
    public decimal MunkaDij { get; set; }   // Ft

    // ── Státusz és dátumok ───────────────────────────────────
    public Statusz Statusz { get; set; } = Statusz.Beerkezett;
    public DateTime BeErkezettDatum { get; set; }
    public DateTime? KeszultDatum { get; set; }

    // ── Számított tulajdonságok ──────────────────────────────
    /// <summary>Összes alkatrész ára</summary>
    public decimal AlkatreszOsszeg => Alkatreszek.Sum(a => a.OsszAr);

    /// <summary>Munka + alkatrészek végösszege</summary>
    public decimal Vegosszeg => AlkatreszOsszeg + MunkaDij;

    /// <summary>Státusz megjelenítése magyarul</summary>
    public string StatuszSzoveg => Statusz switch
    {
        Statusz.Beerkezett   => "📥 Beérkezett",
        Statusz.Diagnosztika => "🔍 Diagnosztika",
        Statusz.Javitas      => "🔧 Javítás alatt",
        Statusz.Kesz         => "✅ Kész",
        Statusz.Kiadva       => "📤 Kiadva",
        _                    => "?"
    };

    /// <summary>Elvégzési idő napokban (null, ha még nincs kész)</summary>
    public int? ElvegzesiNapok => KeszultDatum.HasValue
        ? (int)(KeszultDatum.Value - BeErkezettDatum).TotalDays
        : null;
}
