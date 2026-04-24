namespace PCSherviz.Models;

/// <summary>
/// Egy alkatészt vagy felhasznált anyagot reprezentál egy munkalapon.
/// </summary>
public class Alkatresz
{
    public string Nev { get; set; } = string.Empty;
    public decimal EgysegAr { get; set; }   // Ft
    public int Mennyiseg { get; set; }

    /// <summary>Számított mező: egységár × mennyiség</summary>
    public decimal OsszAr => EgysegAr * Mennyiseg;

    public override string ToString()
        => $"{Nev} ({Mennyiseg} db × {EgysegAr:N0} Ft = {OsszAr:N0} Ft)";
}
