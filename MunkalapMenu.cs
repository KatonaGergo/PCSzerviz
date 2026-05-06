using PCSzerviz.Models;
using PCSzerviz.Services;
using PCSzerviz.UI;

namespace PCSzerviz.Menus;

/// <summary>
/// Az alkalmazás összes menüképernyőjét tartalmazza.
/// </summary>
internal class MunkalapMenu
{
    private readonly MunkalapSzolgaltatas _szolg;

    public MunkalapMenu(MunkalapSzolgaltatas szolgaltatas)
    {
        _szolg = szolgaltatas;
    }

    // ════════════════════════════════════════════════════════
    //  FŐMENÜ
    // ════════════════════════════════════════════════════════

    public void Futtat()
    {
        while (true)
        {
            FomenüKiir();
            string? valasztas = Console.ReadLine()?.Trim();

            try
            {
                switch (valasztas)
                {
                    case "1": UjMunkalapMenu();     break;
                    case "2": ListazasMenu();       break;
                    case "3": KeresMenu();          break;
                    case "4": ReszletekMenu();      break;
                    case "5": StatuszMenu();        break;
                    case "6": AlkatreszMenu();      break;
                    case "7": MunkaDijMenu();       break;
                    case "8": MegjegyzesMenu();     break;
                    case "9": TorlesMenu();         break;
                    case "0": StatisztikaMenu();    break;
                    case "X":
                    case "x":
                        Kilepes();
                        return;
                    default:
                        KonzolSeged.Figyelmeztet("Érvénytelen választás, próbáld újra!");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Nem kezelt kivétel elkapása – a program nem omlik össze
                KonzolSeged.Hiba($"Váratlan hiba: {ex.Message}");
                KonzolSeged.VarEnter();
            }
        }
    }

    private void FomenüKiir()
    {
        Console.Clear();
        KonzolSeged.Fejlec("PC SZERVIZ MUNKALAP-KEZELŐ  v1.0");

        // Gyors áttekintés a főmenüben
        int nyitott = _szolg.NyitottMunkakSzama();
        Console.ForegroundColor = nyitott > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.WriteLine($"  Nyitott munkák: {nyitott} db");
        Console.ResetColor();
        KonzolSeged.Elvalaszto();

        Console.WriteLine("  [1]  Új munkalap felvétele");
        Console.WriteLine("  [2]  Összes munkalap listázása");
        Console.WriteLine("  [3]  Keresés / szűrés");
        Console.WriteLine("  [4]  Munkalap részletei");
        Console.WriteLine("  [5]  Státusz módosítása");
        Console.WriteLine("  [6]  Alkatrész kezelése");
        Console.WriteLine("  [7]  Munkadíj módosítása");
        Console.WriteLine("  [8]  Technikus megjegyzés");
        Console.WriteLine("  [9]  Munkalap törlése");
        Console.WriteLine("  [0]  Statisztikák");
        Console.WriteLine("  [X]  Kilépés");
        KonzolSeged.Elvalaszto();
        Console.Write("  Választás: ");
    }

    // ════════════════════════════════════════════════════════
    //  1 – ÚJ MUNKALAP
    // ════════════════════════════════════════════════════════

    private void UjMunkalapMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("ÚJ MUNKALAP FELVÉTELE");

        try
        {
            string ugyfelNev    = KonzolSeged.OlvasSzoveg("Ügyfél neve");
            string ugyfelTel    = KonzolSeged.OlvasSzoveg("Ügyfél telefonszáma");
            string eszkozTipus  = KonzolSeged.OlvasSzoveg("Eszköz típusa (pl. Laptop, Desktop, Tablet)");
            string eszkozMarka  = KonzolSeged.OlvasSzoveg("Eszköz márkája / modellje");
            string sorozatSzam  = KonzolSeged.OlvasSzoveg("Sorozatszám (SN)");
            string hibaLeiras   = KonzolSeged.OlvasSzoveg("Hibaleírás");
            decimal munkaDij    = KonzolSeged.OlvasPenz("Becsült munkadíj");

            var munkalap = _szolg.UjMunkalap(
                ugyfelNev, ugyfelTel,
                eszkozTipus, eszkozMarka, sorozatSzam,
                hibaLeiras, munkaDij);

            Console.WriteLine();
            KonzolSeged.Siker($"Munkalap sikeresen létrehozva! Azonosító: #{munkalap.Id}");
        }
        catch (ArgumentException ex)
        {
            KonzolSeged.Hiba(ex.Message);
        }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  2 – LISTÁZÁS
    // ════════════════════════════════════════════════════════

    private void ListazasMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("ÖSSZES MUNKALAP");

        var lista = _szolg.OsszesMunkalap()
                          .OrderByDescending(m => m.BeErkezettDatum)
                          .ToList();

        if (lista.Count == 0)
        {
            KonzolSeged.Figyelmeztet("Nincs felvett munkalap.");
        }
        else
        {
            MunkalapTablazat(lista);
        }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  3 – KERESÉS
    // ════════════════════════════════════════════════════════

    private void KeresMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("KERESÉS / SZŰRÉS");

        string? szoveg = KonzolSeged.OlvasOptSzoveg(
            "Keresési szöveg (ügyfél, márka, SN, hiba) – Enter = összes");

        Console.WriteLine();
        Console.WriteLine("  Szűrés státuszra (Enter = összes):");
        Console.WriteLine("  0 = Beérkezett  1 = Diagnosztika  2 = Javítás  3 = Kész  4 = Kiadva");
        string? statuszBev = KonzolSeged.OlvasOptSzoveg("Státusz kódja");

        Statusz? statusz = null;
        if (statuszBev != null && int.TryParse(statuszBev, out int sk) && Enum.IsDefined(typeof(Statusz), sk))
            statusz = (Statusz)sk;

        var eredmeny = _szolg.Kereses(szoveg, statusz);

        Console.WriteLine();
        if (eredmeny.Count == 0)
            KonzolSeged.Figyelmeztet("Nincs találat.");
        else
        {
            KonzolSeged.Info($"Találatok: {eredmeny.Count} db");
            MunkalapTablazat(eredmeny);
        }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  4 – RÉSZLETEK
    // ════════════════════════════════════════════════════════

    private void ReszletekMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("MUNKALAP RÉSZLETEI");

        int id = KonzolSeged.OlvasInt("Munkalap azonosítója (#)", 1);
        var m = _szolg.MunkalapIdAlapjan(id);

        if (m == null)
        {
            KonzolSeged.Hiba($"Nem található munkalap #{id} azonosítóval.");
            KonzolSeged.VarEnter();
            return;
        }

        Console.Clear();
        KonzolSeged.Fejlec($"MUNKALAP #{m.Id}  –  {m.StatuszSzoveg}");

        Console.WriteLine();
        KonzolSeged.Info($"Ügyfél:       {m.UgyfelNev}");
        KonzolSeged.Info($"Telefon:      {m.UgyfelTelefon}");
        KonzolSeged.Info($"Eszköz:       {m.EszkozTipus} – {m.EszkozMarka}");
        KonzolSeged.Info($"Sorozatszám:  {m.SorozatSzam}");
        KonzolSeged.Info($"Beérkezett:   {m.BeErkezettDatum:yyyy.MM.dd HH:mm}");

        if (m.KeszultDatum.HasValue)
            KonzolSeged.Info($"Elkészült:    {m.KeszultDatum:yyyy.MM.dd HH:mm}  ({m.ElvegzesiNapok} nap)");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Hibaleírás:   {m.HibaLeiras}");
        Console.ResetColor();

        if (!string.IsNullOrEmpty(m.TechnikusMegjegyzes))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  Megjegyzés:   {m.TechnikusMegjegyzes}");
            Console.ResetColor();
        }

        KonzolSeged.Elvalaszto();

        if (m.Alkatreszek.Count > 0)
        {
            Console.WriteLine("  ALKATRÉSZEK:");
            foreach (var a in m.Alkatreszek)
                KonzolSeged.Info($"    • {a}");
        }
        else
        {
            KonzolSeged.Info("  Alkatrészek: (nincs)");
        }

        KonzolSeged.Elvalaszto();
        KonzolSeged.Info($"Alkatrész összesen:   {m.AlkatreszOsszeg,10:N0} Ft");
        KonzolSeged.Info($"Munkadíj:             {m.MunkaDij,10:N0} Ft");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  VÉGÖSSZEG:            {m.Vegosszeg,10:N0} Ft");
        Console.ResetColor();

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  5 – STÁTUSZ
    // ════════════════════════════════════════════════════════

    private void StatuszMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("STÁTUSZ MÓDOSÍTÁSA");

        int id = KonzolSeged.OlvasInt("Munkalap azonosítója (#)", 1);
        var m = _szolg.MunkalapIdAlapjan(id);

        if (m == null) { KonzolSeged.Hiba($"Nem található #{id}."); KonzolSeged.VarEnter(); return; }

        KonzolSeged.Info($"Jelenlegi státusz: {m.StatuszSzoveg}");
        Console.WriteLine();
        Console.WriteLine("  Elérhető állapotok:");
        Console.WriteLine("  0 = Beérkezett  1 = Diagnosztika  2 = Javítás  3 = Kész  4 = Kiadva");

        int ujKod = KonzolSeged.OlvasInt("Új státusz kódja", 0, 4);

        try
        {
            _szolg.StatuszFrissites(id, (Statusz)ujKod);
            KonzolSeged.Siker("Státusz frissítve!");
        }
        catch (InvalidOperationException ex)
        {
            KonzolSeged.Hiba(ex.Message);
        }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  6 – ALKATRÉSZEK
    // ════════════════════════════════════════════════════════

    private void AlkatreszMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("ALKATRÉSZ KEZELÉSE");

        int id = KonzolSeged.OlvasInt("Munkalap azonosítója (#)", 1);
        var m = _szolg.MunkalapIdAlapjan(id);

        if (m == null) { KonzolSeged.Hiba($"Nem található #{id}."); KonzolSeged.VarEnter(); return; }

        Console.WriteLine();
        Console.WriteLine("  [1] Alkatrész hozzáadása");
        Console.WriteLine("  [2] Alkatrész eltávolítása");
        Console.WriteLine("  [0] Vissza");
        Console.Write("  Választás: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                try
                {
                    string nev     = KonzolSeged.OlvasSzoveg("Alkatrész neve");
                    decimal ar     = KonzolSeged.OlvasPenz("Egységár");
                    int db         = KonzolSeged.OlvasInt("Mennyiség (db)", 1, 9999);
                    _szolg.AlkatreszHozzaad(id, nev, ar, db);
                    KonzolSeged.Siker("Alkatrész hozzáadva!");
                }
                catch (Exception ex) { KonzolSeged.Hiba(ex.Message); }
                break;

            case "2":
                if (m.Alkatreszek.Count == 0)
                {
                    KonzolSeged.Figyelmeztet("Nincs alkatrész ezen a munkalapon.");
                    break;
                }
                Console.WriteLine("  Meglévő alkatrészek:");
                m.Alkatreszek.ForEach(a => KonzolSeged.Info($"  • {a.Nev}"));
                try
                {
                    string nev = KonzolSeged.OlvasSzoveg("Eltávolítandó alkatrész neve");
                    _szolg.AlkatreszEltavolit(id, nev);
                    KonzolSeged.Siker("Alkatrész eltávolítva!");
                }
                catch (Exception ex) { KonzolSeged.Hiba(ex.Message); }
                break;
        }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  7 – MUNKADÍJ
    // ════════════════════════════════════════════════════════

    private void MunkaDijMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("MUNKADÍJ MÓDOSÍTÁSA");

        int id = KonzolSeged.OlvasInt("Munkalap azonosítója (#)", 1);
        var m = _szolg.MunkalapIdAlapjan(id);

        if (m == null) { KonzolSeged.Hiba($"Nem található #{id}."); KonzolSeged.VarEnter(); return; }

        KonzolSeged.Info($"Jelenlegi munkadíj: {m.MunkaDij:N0} Ft");

        try
        {
            decimal ujDij = KonzolSeged.OlvasPenz("Új munkadíj");
            _szolg.MunkaDijFrissites(id, ujDij);
            KonzolSeged.Siker("Munkadíj frissítve!");
        }
        catch (Exception ex) { KonzolSeged.Hiba(ex.Message); }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  8 – MEGJEGYZÉS
    // ════════════════════════════════════════════════════════

    private void MegjegyzesMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("TECHNIKUS MEGJEGYZÉS");

        int id = KonzolSeged.OlvasInt("Munkalap azonosítója (#)", 1);
        var m = _szolg.MunkalapIdAlapjan(id);

        if (m == null) { KonzolSeged.Hiba($"Nem található #{id}."); KonzolSeged.VarEnter(); return; }

        if (!string.IsNullOrEmpty(m.TechnikusMegjegyzes))
            KonzolSeged.Info($"Jelenlegi megjegyzés: {m.TechnikusMegjegyzes}");

        string? uj = KonzolSeged.OlvasOptSzoveg("Új megjegyzés (Enter = törlés)");
        _szolg.MegjegyzesFrissites(id, uj);
        KonzolSeged.Siker(uj == null ? "Megjegyzés törölve." : "Megjegyzés mentve!");

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  9 – TÖRLÉS
    // ════════════════════════════════════════════════════════

    private void TorlesMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("MUNKALAP TÖRLÉSE");

        int id = KonzolSeged.OlvasInt("Munkalap azonosítója (#)", 1);
        var m = _szolg.MunkalapIdAlapjan(id);

        if (m == null) { KonzolSeged.Hiba($"Nem található #{id}."); KonzolSeged.VarEnter(); return; }

        Console.WriteLine();
        KonzolSeged.Figyelmeztet($"Törlendő munkalap: #{m.Id} – {m.UgyfelNev} / {m.EszkozMarka}");

        if (m.Statusz == Statusz.Kiadva)
            KonzolSeged.Figyelmeztet("Ez a munkalap már ki van adva!");

        bool biztos = KonzolSeged.OlvasIgenNem("Biztosan törlöd?");
        if (!biztos) { KonzolSeged.Info("Törlés megszakítva."); KonzolSeged.VarEnter(); return; }

        try
        {
            _szolg.Torles(id, kiadottIsTorolheto: m.Statusz == Statusz.Kiadva);
            KonzolSeged.Siker("Munkalap törölve!");
        }
        catch (InvalidOperationException ex)
        {
            KonzolSeged.Hiba(ex.Message);
        }

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  0 – STATISZTIKÁK
    // ════════════════════════════════════════════════════════

    private void StatisztikaMenu()
    {
        Console.Clear();
        KonzolSeged.Fejlec("STATISZTIKÁK");

        var osszes   = _szolg.OsszesMunkalap();
        var eloszlas = _szolg.StatuszEloszlas();
        double? atlag = _szolg.AtlagosElvegzesiIdoNap();
        decimal bevetel = _szolg.OsszBevetal();

        Console.WriteLine();
        KonzolSeged.Info($"Összes munkalap:          {osszes.Count} db");
        KonzolSeged.Info($"Nyitott munkák:           {_szolg.NyitottMunkakSzama()} db");
        Console.WriteLine();

        Console.WriteLine("  Státusz szerinti bontás:");
        foreach (Statusz s in Enum.GetValues<Statusz>())
        {
            int db = eloszlas.TryGetValue(s, out int d) ? d : 0;
            string sor = $"    {s,-15} : {db,3} db";
            Console.WriteLine(sor);
        }

        Console.WriteLine();
        KonzolSeged.Info($"Átl. elvégzési idő:       {(atlag.HasValue ? $"{atlag:F1} nap" : "–")}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Összesített bevétel:      {bevetel:N0} Ft  (kiadott munkák)");
        Console.ResetColor();

        KonzolSeged.VarEnter();
    }

    // ════════════════════════════════════════════════════════
    //  SEGÉD: TÁBLÁZATOS MEGJELENÍTÉS
    // ════════════════════════════════════════════════════════

    private static void MunkalapTablazat(IEnumerable<Munkalap> lista)
    {
        Console.WriteLine();
        string fejlec = $"  {"#",-5} {"Ügyfél",-20} {"Eszköz",-22} {"Végösszeg",10}  {"Dátum",12}  Státusz";
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(fejlec);
        Console.WriteLine("  " + new string('─', fejlec.Length - 2));
        Console.ResetColor();

        foreach (var m in lista)
        {
            bool kesz = m.Statusz == Statusz.Kesz || m.Statusz == Statusz.Kiadva;
            Console.ForegroundColor = kesz ? ConsoleColor.Green :
                                      m.Statusz == Statusz.Javitas ? ConsoleColor.Yellow :
                                      ConsoleColor.Gray;

            string eszkoz = $"{m.EszkozTipus} {m.EszkozMarka}";
            if (eszkoz.Length > 21) eszkoz = eszkoz[..18] + "...";

            Console.WriteLine(
                $"  {m.Id,-5} " +
                $"{Csonkit(m.UgyfelNev, 19),-20} " +
                $"{eszkoz,-22} " +
                $"{m.Vegosszeg,9:N0} Ft  " +
                $"{m.BeErkezettDatum:yyyy.MM.dd}  " +
                $"{m.StatuszSzoveg}");
        }
        Console.ResetColor();
    }

    private static string Csonkit(string s, int max)
        => s.Length > max ? s[..(max - 3)] + "..." : s;

    // ════════════════════════════════════════════════════════
    //  KILÉPÉS
    // ════════════════════════════════════════════════════════

    private void Kilepes()
    {
        try
        {
            _szolg.MentFajlba();
            Console.WriteLine();
            KonzolSeged.Siker("Adatok mentve. Viszlát!");
        }
        catch (Exception ex)
        {
            KonzolSeged.Hiba($"Mentési hiba kilépéskor: {ex.Message}");
        }
    }
}

