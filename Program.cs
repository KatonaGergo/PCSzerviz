using PCSzerviz.Menus;
using PCSzerviz.Services;
using PCSzerviz.UI;

// ── Konzol beállítása ─────────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "PC Szerviz Munkalap-kezelő";

// ── Adatfájl útvonala ─────────────────────────────────────
string adatfajl = args.Length > 0 ? args[0] : "munkalapok.json";

// ── Betöltés ──────────────────────────────────────────────
MunkalapSzolgaltatas szolgaltatas;

try
{
    szolgaltatas = new MunkalapSzolgaltatas(adatfajl);
    KonzolSeged.Siker($"Adatok betöltve: {adatfajl}");
    System.Threading.Thread.Sleep(800);
}
catch (Exception ex)
{
    KonzolSeged.Hiba($"Kritikus hiba az induláskor: {ex.Message}");
    Console.WriteLine("Nyomj Entert a kilépéshez...");
    Console.ReadLine();
    return;
}

// ── Főmenü indítása ───────────────────────────────────────
var menu = new MunkalapMenu(szolgaltatas);
menu.Futtat();
