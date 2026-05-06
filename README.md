# PC Szerviz Munkalap-kezelő 🔧

Konzolalkalmazás C# (.NET 8) – Szerviz munkák nyilvántartása.

---

## Indítás

```bash
dotnet run --project PCSzerviz.csproj
# vagy saját adatfájllal:
dotnet run -- sajat_adatok.json
```

---

## Funkciók

### CRUD
| Funkció | Leírás |
|---|---|
| **Create** | Új munkalap felvétele (ügyfél, eszköz, hiba, munkadíj) |
| **Read** | Lista + szövegalapú keresés + státusz szűrő + részletes nézet |
| **Update** | Státusz léptető, alkatrész hozzáadás/törlés, munkadíj, megjegyzés |
| **Delete** | Törlés megerősítéssel (kiadott munka extra figyelmezetéssel) |

### Fájlkezelés
- Adatok **JSON** formátumban mentve (`munkalapok.json`)
- Betöltés **induláskor**, mentés **minden módosítás után** (elveszett adat = nulla)

### Validáció (az „Excel-killer" rész)
| Szabály | Hol van? |
|---|---|
| Negatív ár/munkadíj tiltott | `MunkalapSzolgaltatas.ValidaljPenzosszeg` |
| Telefonszám 7–15 számjegy | `ValidaljTelefon` |
| Ügyfélnév 2–100 kar. | `ValidaljUgyfelNev` |
| Hibaleírás min. 5 kar. | `ValidaljHiba` |
| Státusz nem léphet vissza | `StatuszFrissites` |
| Kiadva → csak Kész után | `StatuszFrissites` |
| Kiadott munkalap nem szerkeszthető | alkatrész/díj metódusok |
| Hibás JSON fájl nem lövi le az appot | `BetoltFajlbol` try-catch |
| Bármilyen rossz konzolos bevitel | `KonzolSeged` ciklus validáció |

### Üzleti logika (statisztikák)
- Nyitott munkák száma (főmenü fejléc)
- Státusz szerinti eloszlás
- Átlagos elvégzési idő (napokban)
- Összesített bevétel (kiadott munkák)

---

## Projektstruktúra

```
PCSzerviz/
├── Models/
│   ├── Alkatresz.cs       – Alkatrész adatmodell
│   └── Munkalap.cs        – Munkalap + Statusz enum
├── Services/
│   └── MunkalapSzolgaltatas.cs  – CRUD, validáció, JSON I/O
├── UI/
│   └── KonzolSeged.cs     – Színes kiírás, validált bevitel
├── Menus/
│   └── MunkalapMenu.cs    – Főmenü + almenük
├── Program.cs             – Belépési pont
└── PCSzerviz.csproj
```

---

## Adatfájl formátum (JSON részlet)

```json
[
  {
    "Id": 1,
    "UgyfelNev": "Kovács István",
    "UgyfelTelefon": "+36301234567",
    "EszkozTipus": "Laptop",
    "EszkozMarka": "Lenovo ThinkPad E14",
    "SorozatSzam": "SN1234XYZ",
    "HibaLeiras": "Nem kapcsol be, töltő nem tölt",
    "Alkatreszek": [
      { "Nev": "Töltő adapter 65W", "EgysegAr": 4500, "Mennyiseg": 1 }
    ],
    "MunkaDij": 3000,
    "Statusz": "Kesz",
    "BeErkezettDatum": "2025-04-20T10:30:00",
    "KeszultDatum": "2025-04-22T14:00:00"
  }
]
```

