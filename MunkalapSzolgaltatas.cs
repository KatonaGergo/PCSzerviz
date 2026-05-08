using System.Text.Json;
using System.Text.Json.Serialization;
using PCSzerviz.Models;

namespace PCSzerviz.Services;

/// <summary>
/// CRUD műveletek
/// </summary>
public class MunkalapSzolgaltatas
{
    // ── Konfiguráció ─────────────────────────────────────────
    private readonly string _adatfajlUtvonal;

    private static readonly JsonSerializerOptions _jsonBeallitasok = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Memóriában tárolt adatok ─────────────────────────────
    private List<Munkalap> _munkalapok = new();

    public MunkalapSzolgaltatas(string adatfajlUtvonal = "munkalapok.json")
    {
        _adatfajlUtvonal = adatfajlUtvonal;
        BetoltFajlbol();
    }

    // ════════════════════════════════════════════════════════
    //  FÁJLKEZELÉS
    // ════════════════════════════════════════════════════════

    /// <summary>JSON fájlból betöltés induláskor.</summary>
    public void BetoltFajlbol()
    {
        try
        {
            if (!File.Exists(_adatfajlUtvonal))
            {
                _munkalapok = new List<Munkalap>();
                return;
            }

            string json = File.ReadAllText(_adatfajlUtvonal);

            if (string.IsNullOrWhiteSpace(json))
            {
                _munkalapok = new List<Munkalap>();
                return;
            }

            _munkalapok = JsonSerializer.Deserialize<List<Munkalap>>(json, _jsonBeallitasok)
                          ?? new List<Munkalap>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[FIGYELMEZTETÉS] Hibás JSON fájl, üres adatbázissal indulunk: {ex.Message}");
            _munkalapok = new List<Munkalap>();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[FIGYELMEZTETÉS] Nem sikerült beolvasni a fájlt: {ex.Message}");
            _munkalapok = new List<Munkalap>();
        }
    }

    /// <summary>Minden módosítás után/kilépéskor mentés JSON-ba.</summary>
    public void MentFajlba()
    {
        try
        {
            string json = JsonSerializer.Serialize(_munkalapok, _jsonBeallitasok);
            File.WriteAllText(_adatfajlUtvonal, json);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Mentési hiba: {ex.Message}", ex);
        }
    }

    // ════════════════════════════════════════════════════════
    //  CREATE
    // ════════════════════════════════════════════════════════

    /// <summary>Új munkalap létrehozása és mentése.</summary>
    public Munkalap UjMunkalap(
        string ugyfelNev,
        string ugyfelTelefon,
        string eszkozTipus,
        string eszkozMarka,
        string sorozatSzam,
        string hibaLeiras,
        decimal munkaDij)
    {
        ValidaljUgyfelNev(ugyfelNev);
        ValidaljTelefon(ugyfelTelefon);
        ValidaljEszkoz(eszkozTipus, eszkozMarka);
        ValidaljHiba(hibaLeiras);
        ValidaljPenzosszeg(munkaDij, nameof(munkaDij), "Munkadíj");

        int ujId = _munkalapok.Count > 0
            ? _munkalapok.Max(m => m.Id) + 1
            : 1;

        var munkalap = new Munkalap
        {
            Id              = ujId,
            UgyfelNev       = ugyfelNev.Trim(),
            UgyfelTelefon   = ugyfelTelefon.Trim(),
            EszkozTipus     = eszkozTipus.Trim(),
            EszkozMarka     = eszkozMarka.Trim(),
            SorozatSzam     = sorozatSzam.Trim(),
            HibaLeiras      = hibaLeiras.Trim(),
            MunkaDij        = munkaDij,
            Statusz         = Statusz.Beerkezett,
            BeErkezettDatum = DateTime.Now
        };

        _munkalapok.Add(munkalap);
        MentFajlba();
        return munkalap;
    }

    // ════════════════════════════════════════════════════════
    //  READ
    // ════════════════════════════════════════════════════════

    /// <summary>Összes munkalap visszaadása.</summary>
    public IReadOnlyList<Munkalap> OsszesMunkalap()
        => _munkalapok.AsReadOnly();

    /// <summary>Egyedi munkalap lekérése ID alapján.</summary>
    public Munkalap? MunkalapIdAlapjan(int id)
        => _munkalapok.FirstOrDefault(m => m.Id == id);

    /// <summary>
    /// Szűrés: ügyfélnévre, eszközmárkára, sorozatszámra
    /// és/vagy státuszra. Minden paraméter opcionális.
    /// </summary>
    public List<Munkalap> Kereses(
        string? szoveg = null,
        Statusz? statusz = null)
    {
        IEnumerable<Munkalap> eredmeny = _munkalapok;

        if (!string.IsNullOrWhiteSpace(szoveg))
        {
            string kisbetu = szoveg.Trim().ToLower();
            eredmeny = eredmeny.Where(m =>
                m.UgyfelNev.ToLower().Contains(kisbetu)    ||
                m.EszkozMarka.ToLower().Contains(kisbetu)  ||
                m.SorozatSzam.ToLower().Contains(kisbetu)  ||
                m.HibaLeiras.ToLower().Contains(kisbetu));
        }

        if (statusz.HasValue)
            eredmeny = eredmeny.Where(m => m.Statusz == statusz.Value);

        return eredmeny.OrderByDescending(m => m.BeErkezettDatum).ToList();
    }

    // ════════════════════════════════════════════════════════
    //  UPDATE – Státusz
    // ════════════════════════════════════════════════════════

    /// <summary>Státusz frissítése</summary>
    public void StatuszFrissites(int id, Statusz ujStatusz)
    {
        var munkalap = MunkalapLeker(id);

        if (ujStatusz == Statusz.Kiadva && munkalap.Statusz != Statusz.Kesz)
            throw new InvalidOperationException(
                "Csak Kész állapotú munkát lehet Kiadva-ra állítani!");


        if ((int)ujStatusz < (int)munkalap.Statusz)
            throw new InvalidOperationException(
                $"Nem lehet visszaléptetni ({munkalap.StatuszSzoveg} → {ujStatusz})!");

        munkalap.Statusz = ujStatusz;

        if (ujStatusz == Statusz.Kesz && munkalap.KeszultDatum == null)
            munkalap.KeszultDatum = DateTime.Now;

        MentFajlba();
    }

    // ════════════════════════════════════════════════════════
    //  UPDATE – Alkatrész hozzáadása
    // ════════════════════════════════════════════════════════

    /// <summary>Alkatrész felvétele egy munkalapon.</summary>
    public void AlkatreszHozzaad(int id, string nev, decimal egysegAr, int mennyiseg)
    {
        ValidaljPenzosszeg(egysegAr, "egysegAr", "Egységár");
        if (mennyiseg <= 0)
            throw new ArgumentException("A mennyiség legalább 1 kell legyen.", nameof(mennyiseg));
        if (string.IsNullOrWhiteSpace(nev))
            throw new ArgumentException("Az alkatrész neve nem lehet üres.", nameof(nev));

        var munkalap = MunkalapLeker(id);

        if (munkalap.Statusz == Statusz.Kiadva)
            throw new InvalidOperationException("Kiadott munkalapon nem módosítható az alkatrészlista!");

        // Ha már van ilyen nevű alkatrész → mennyiség növelése
        var meglevo = munkalap.Alkatreszek
            .FirstOrDefault(a => a.Nev.ToLower() == nev.Trim().ToLower());

        if (meglevo != null)
            meglevo.Mennyiseg += mennyiseg;
        else
            munkalap.Alkatreszek.Add(new Alkatresz
            {
                Nev        = nev.Trim(),
                EgysegAr   = egysegAr,
                Mennyiseg  = mennyiseg
            });

        MentFajlba();
    }

    /// <summary>Alkatrész eltávolítása a munkalapon.</summary>
    public void AlkatreszEltavolit(int id, string nev)
    {
        var munkalap = MunkalapLeker(id);

        if (munkalap.Statusz == Statusz.Kiadva)
            throw new InvalidOperationException("Kiadott munkalapon nem módosítható az alkatrészlista!");

        var alkatresz = munkalap.Alkatreszek
            .FirstOrDefault(a => a.Nev.ToLower() == nev.Trim().ToLower())
            ?? throw new KeyNotFoundException($"Nem található ilyen alkatrész: '{nev}'");

        munkalap.Alkatreszek.Remove(alkatresz);
        MentFajlba();
    }

    // ════════════════════════════════════════════════════════
    //  UPDATE – Munkadíj / Technikus megjegyzés
    // ════════════════════════════════════════════════════════

    public void MunkaDijFrissites(int id, decimal ujDij)
    {
        ValidaljPenzosszeg(ujDij, "ujDij", "Munkadíj");
        var munkalap = MunkalapLeker(id);

        if (munkalap.Statusz == Statusz.Kiadva)
            throw new InvalidOperationException("Kiadott munkalap díja nem módosítható!");

        munkalap.MunkaDij = ujDij;
        MentFajlba();
    }

    public void MegjegyzesFrissites(int id, string? megjegyzes)
    {
        var munkalap = MunkalapLeker(id);
        munkalap.TechnikusMegjegyzes = megjegyzes?.Trim();
        MentFajlba();
    }

    // ════════════════════════════════════════════════════════
    //  DELETE / INAKTIVÁLÁS
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Munkalap törlése. Kiadva állapotú csak megerősítéssel törölhető.
    /// </summary>
    public void Torles(int id, bool kiadottIsTorolheto = false)
    {
        var munkalap = MunkalapLeker(id);

        if (munkalap.Statusz == Statusz.Kiadva && !kiadottIsTorolheto)
            throw new InvalidOperationException(
                "Kiadott munkalap nem törölhető a megerősítő jelző nélkül!");

        _munkalapok.Remove(munkalap);
        MentFajlba();
    }

    // ════════════════════════════════════════════════════════
    //  STATISZTIKÁK (üzleti logika)
    // ════════════════════════════════════════════════════════

    /// <summary>Nyitott (nem kiadott) munkák száma.</summary>
    public int NyitottMunkakSzama()
        => _munkalapok.Count(m => m.Statusz != Statusz.Kiadva);

    /// <summary>Átlagos elvégzési idő napokban (csak befejezett munkákra).</summary>
    public double? AtlagosElvegzesiIdoNap()
    {
        var befejezettNapok = _munkalapok
            .Where(m => m.ElvegzesiNapok.HasValue)
            .Select(m => m.ElvegzesiNapok!.Value)
            .ToList();

        return befejezettNapok.Count > 0
            ? befejezettNapok.Average()
            : null;
    }

    /// <summary>Összesített bevétel (kiadott munkák végösszege).</summary>
    public decimal OsszBevetal()
        => _munkalapok
            .Where(m => m.Statusz == Statusz.Kiadva)
            .Sum(m => m.Vegosszeg);

    /// <summary>Státuszonként csoportosított darabszám.</summary>
    public Dictionary<Statusz, int> StatuszEloszlas()
        => _munkalapok
            .GroupBy(m => m.Statusz)
            .ToDictionary(g => g.Key, g => g.Count());

    // ════════════════════════════════════════════════════════
    //  PRIVÁT SEGÉDMETÓDUSOK
    // ════════════════════════════════════════════════════════

    private Munkalap MunkalapLeker(int id)
        => _munkalapok.FirstOrDefault(m => m.Id == id)
           ?? throw new KeyNotFoundException($"Nem található munkalap #{id} azonosítóval.");

    private static void ValidaljUgyfelNev(string nev)
    {
        if (string.IsNullOrWhiteSpace(nev))
            throw new ArgumentException("Az ügyfél neve nem lehet üres.", nameof(nev));
        if (nev.Trim().Length < 2)
            throw new ArgumentException("Az ügyfél neve legalább 2 karakter legyen.", nameof(nev));
        if (nev.Trim().Length > 100)
            throw new ArgumentException("Az ügyfél neve legfeljebb 100 karakter lehet.", nameof(nev));
    }

    private static void ValidaljTelefon(string tel)
    {
        if (string.IsNullOrWhiteSpace(tel))
            throw new ArgumentException("A telefonszám nem lehet üres.", nameof(tel));

        string csak_szamok = new string(tel.Where(char.IsDigit).ToArray());
        if (csak_szamok.Length < 7 || csak_szamok.Length > 15)
            throw new ArgumentException("A telefonszám 7–15 számjegyből állhat.", nameof(tel));
    }

    private static void ValidaljEszkoz(string tipus, string marka)
    {
        if (string.IsNullOrWhiteSpace(tipus))
            throw new ArgumentException("Az eszköz típusa nem lehet üres.", nameof(tipus));
        if (string.IsNullOrWhiteSpace(marka))
            throw new ArgumentException("Az eszköz márkája/modellje nem lehet üres.", nameof(marka));
    }

    private static void ValidaljHiba(string leiras)
    {
        if (string.IsNullOrWhiteSpace(leiras))
            throw new ArgumentException("A hibaleírás nem lehet üres.", nameof(leiras));
        if (leiras.Trim().Length < 5)
            throw new ArgumentException("A hibaleírás legalább 5 karakter legyen.", nameof(leiras));
    }

    private static void ValidaljPenzosszeg(decimal osszeg, string paramNev, string megjelenitesNev)
    {
        if (osszeg < 0)
            throw new ArgumentException($"{megjelenitesNev} nem lehet negatív!", paramNev);
        if (osszeg > 10_000_000)
            throw new ArgumentException($"{megjelenitesNev} nem lehet több 10 millió Ft-nál!", paramNev);
    }
}

