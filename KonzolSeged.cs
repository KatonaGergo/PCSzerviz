namespace PCSzerviz.UI;

/// <summary>
/// Konzol I/O segédmetódusok
/// </summary>
internal static class KonzolSeged
{
    // ── Kiírás ───────────────────────────────────────────────

    public static void Fejlec(string szoveg)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔" + new string('═', szoveg.Length + 2) + "╗");
        Console.WriteLine($"║ {szoveg} ║");
        Console.WriteLine("╚" + new string('═', szoveg.Length + 2) + "╝");
        Console.ResetColor();
    }

    public static void Siker(string uzenet)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✔ {uzenet}");
        Console.ResetColor();
    }

    public static void Hiba(string uzenet)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✖ HIBA: {uzenet}");
        Console.ResetColor();
    }

    public static void Figyelmeztet(string uzenet)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {uzenet}");
        Console.ResetColor();
    }

    public static void Info(string uzenet)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"  {uzenet}");
        Console.ResetColor();
    }

    public static void Elvalaszto()
        => Console.WriteLine(new string('─', 60));

    // ── Bevitel ──────────────────────────────────────────────

    /// <summary>Szöveget kér be; null visszatérés = üres (opcionális mezőknél).</summary>
    public static string? OlvasOptSzoveg(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"  {prompt}: ");
        Console.ResetColor();
        string? bev = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(bev) ? null : bev;
    }

    /// <summary>Kötelező szöveget kér be, nem engedi üresen hagyni.</summary>
    public static string OlvasSzoveg(string prompt)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {prompt}: ");
            Console.ResetColor();
            string? bev = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(bev)) return bev;
            Hiba("Ez a mező nem maradhat üres!");
        }
    }

    /// <summary>Egész számot kér be a megadott [min, max] tartományon belül.</summary>
    public static int OlvasInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {prompt}: ");
            Console.ResetColor();
            string? bev = Console.ReadLine()?.Trim();

            if (int.TryParse(bev, out int ertek) && ertek >= min && ertek <= max)
                return ertek;

            Hiba($"Érvénytelen szám! Adj meg egy egész számot ({min} – {max}).");
        }
    }

    /// <summary>Pénzösszeget kér be (nem negatív decimal).</summary>
    public static decimal OlvasPenz(string prompt)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {prompt} (Ft): ");
            Console.ResetColor();
            string? bev = Console.ReadLine()?.Replace(",", ".")?.Trim();

            if (decimal.TryParse(bev,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal ertek) && ertek >= 0)
                return ertek;

            Hiba("Érvénytelen összeg! Add meg tizedes számként (pl. 1500 vagy 1500.50).");
        }
    }

    /// <summary>Igen/Nem kérdés: 'i'/'n' gomb.</summary>
    public static bool OlvasIgenNem(string kerdes)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {kerdes} [i/n]: ");
            Console.ResetColor();
            string? bev = Console.ReadLine()?.Trim().ToLower();
            if (bev == "i") return true;
            if (bev == "n") return false;
            Hiba("Kérlek 'i' (igen) vagy 'n' (nem) gombbal válaszolj!");
        }
    }

    /// <summary>Enter billentyű megnyomásáig vár.</summary>
    public static void VarEnter()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("\n  Nyomj Entert a folytatáshoz...");
        Console.ResetColor();
        Console.ReadLine();
    }
}

