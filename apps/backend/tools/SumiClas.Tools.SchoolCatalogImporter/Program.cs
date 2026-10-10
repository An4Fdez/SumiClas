// One-off tool: loads the official school lists (three legacy .xls files) into the dev database.
// Idempotent: rows are matched by natural key (province normalized name, (province, locality normalized name),
// (locality, postal code), school official code), so running it again changes nothing.
// Usage: SumiClas.Tools.SchoolCatalogImporter [directoryWithTheXlsFiles] [connectionString]
// The connection string defaults to env var SUMICLAS_DB, then to the API's appsettings.Development.json.
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using SumiClas.Localities.Domain;
using SumiClas.PostalCodes.Domain;
using SumiClas.Provinces.Domain;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

const int ProvinceColumn = 1, LocalityColumn = 2, GenericColumn = 3, SpecificColumn = 4, CodeColumn = 5,
    AddressColumn = 7, PostalColumn = 8, PhoneColumn = 9;
const int ChunkSize = 1000;
const int MaxPhoneLength = 20;
const int MaxAddressLength = 300;
const int MaxNameLength = 200;
string[] fileNames = ["listado_centros_primaria.xls", "listado_centros_secundaria.xls", "listado_centros_bachillerato.xls"];

var directory = args.Length > 0 ? args[0] : @"C:\Users\anusk\Desktop";
var connectionString = args.Length > 1
    ? args[1]
    : Environment.GetEnvironmentVariable("SUMICLAS_DB") ?? ReadDevConnectionString();

// ---------- 1. Read and merge the Excel files ----------
var merged = new Dictionary<string, SourceRow>();
var rowsRead = 0;
var conflicts = 0;
foreach (var fileName in fileNames)
{
    var path = Path.Combine(directory, fileName);
    if (!File.Exists(path))
    {
        Console.Error.WriteLine($"Missing file: {path}");
        return 1;
    }

    using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    using var reader = ExcelReaderFactory.CreateReader(stream);
    var fileRows = 0;
    while (reader.Read())
    {
        var code = Cell(reader, CodeColumn);
        if (!Regex.IsMatch(code, @"^\d{8}$"))
        {
            continue; // header rows (also the one repeated at the end of each file) and blanks
        }

        fileRows++;
        var row = new SourceRow(
            Cell(reader, ProvinceColumn),
            Cell(reader, LocalityColumn),
            Cell(reader, GenericColumn),
            Cell(reader, SpecificColumn),
            code,
            Cell(reader, AddressColumn),
            Cell(reader, PostalColumn, padTo: 5),
            Cell(reader, PhoneColumn));

        if (merged.TryGetValue(code, out var existing))
        {
            if (existing.ConflictsWith(row)) conflicts++;
            merged[code] = existing.FillGapsFrom(row);
        }
        else
        {
            merged[code] = row;
        }
    }

    rowsRead += fileRows;
    Console.WriteLine($"{fileName}: {fileRows} school rows");
}

Console.WriteLine($"Rows read: {rowsRead} | unique center codes: {merged.Count} | cross-file conflicts on non-empty fields: {conflicts}");

// ---------- 2. Build the territory (spelling variants collapse by normalized name) ----------
var usable = new List<SourceRow>();
var skippedNoTerritory = 0;
var skippedNoName = 0;
foreach (var row in merged.Values.OrderBy(r => r.Code, StringComparer.Ordinal))
{
    if (TextNormalizer.Normalize(row.Province).Length == 0 || TextNormalizer.Normalize(row.Locality).Length == 0)
    {
        skippedNoTerritory++;
        continue;
    }
    if (FullName(row).Length == 0)
    {
        skippedNoName++;
        continue;
    }
    usable.Add(row);
}

var provinceSpellings = new Dictionary<string, Dictionary<string, int>>();
var localitySpellings = new Dictionary<(string Province, string Locality), Dictionary<string, int>>();
var postalPairs = new HashSet<(string Province, string Locality, string Code)>();
foreach (var row in usable)
{
    var provinceKey = TextNormalizer.Normalize(row.Province);
    var localityKey = TextNormalizer.Normalize(row.Locality);
    Count(provinceSpellings, provinceKey, row.Province);
    Count(localitySpellings, (provinceKey, localityKey), row.Locality);
    if (IsValidPostal(row.Postal))
    {
        postalPairs.Add((provinceKey, localityKey, row.Postal));
    }
}

Console.WriteLine(
    $"Source totals: schools {usable.Count} | provinces {provinceSpellings.Count} | localities {localitySpellings.Count} | " +
    $"(locality, postal code) pairs {postalPairs.Count} | skipped without province/locality {skippedNoTerritory}, without name {skippedNoName}");

// ---------- 3. Load into the database (one transaction) ----------
var options = new DbContextOptionsBuilder<SumiClasDbContext>().UseNpgsql(connectionString).Options;
await using var db = new SumiClasDbContext(options);
db.ChangeTracker.AutoDetectChangesEnabled = false;
await using var transaction = await db.Database.BeginTransactionAsync();

// Provinces
var provinces = await db.Provinces.ToDictionaryAsync(p => p.NormalizedName);
var newProvinces = 0;
foreach (var (key, spellings) in provinceSpellings.OrderBy(p => p.Key, StringComparer.Ordinal))
{
    if (provinces.ContainsKey(key)) continue;
    var province = Province.Create(Guid.NewGuid(), MostFrequent(spellings)).Value;
    provinces[key] = province;
    db.Provinces.Add(province);
    newProvinces++;
}
await db.SaveChangesAsync();

// Localities
var localities = (await db.Localities.ToListAsync()).ToDictionary(l => (l.ProvinceId, l.NormalizedName));
var newLocalities = 0;
foreach (var ((provinceKey, localityKey), spellings) in localitySpellings
             .OrderBy(l => l.Key.Province, StringComparer.Ordinal).ThenBy(l => l.Key.Locality, StringComparer.Ordinal))
{
    var provinceId = provinces[provinceKey].Id;
    if (localities.ContainsKey((provinceId, localityKey))) continue;
    var locality = Locality.Create(Guid.NewGuid(), provinceId, MostFrequent(spellings)).Value;
    localities[(provinceId, localityKey)] = locality;
    db.Localities.Add(locality);
    newLocalities++;
    if (newLocalities % ChunkSize == 0) await db.SaveChangesAsync();
}
await db.SaveChangesAsync();

// Postal codes
var postalCodes = (await db.PostalCodes.ToListAsync()).ToDictionary(p => (p.LocalityId, p.Code));
var newPostalCodes = 0;
foreach (var (provinceKey, localityKey, code) in postalPairs
             .OrderBy(p => p.Province, StringComparer.Ordinal).ThenBy(p => p.Locality, StringComparer.Ordinal).ThenBy(p => p.Code, StringComparer.Ordinal))
{
    var localityId = localities[(provinces[provinceKey].Id, localityKey)].Id;
    if (postalCodes.ContainsKey((localityId, code))) continue;
    var postalCode = PostalCode.Create(Guid.NewGuid(), localityId, code).Value;
    postalCodes[(localityId, code)] = postalCode;
    db.PostalCodes.Add(postalCode);
    newPostalCodes++;
    if (newPostalCodes % ChunkSize == 0) await db.SaveChangesAsync();
}
await db.SaveChangesAsync();

// Schools
var schools = await db.Schools.ToDictionaryAsync(s => s.OfficialCode);
var newSchools = 0;
var updatedSchools = 0;
var invalidSchools = 0;
var phonesDropped = 0;
var addressesTruncated = 0;
var pending = 0;
foreach (var row in usable)
{
    var localityId = localities[(provinces[TextNormalizer.Normalize(row.Province)].Id, TextNormalizer.Normalize(row.Locality))].Id;
    Guid? postalCodeId = IsValidPostal(row.Postal) ? postalCodes[(localityId, row.Postal)].Id : null;

    var phone = row.Phone.Length == 0 ? null : row.Phone;
    if (phone is { Length: > MaxPhoneLength })
    {
        phone = null;
        phonesDropped++;
    }

    var address = row.Address.Length == 0 ? null : row.Address;
    if (address is { Length: > MaxAddressLength })
    {
        address = address[..MaxAddressLength];
        addressesTruncated++;
    }

    var name = FullName(row);
    if (name.Length > MaxNameLength)
    {
        name = name[..MaxNameLength];
    }

    var candidate = School.Create(Guid.NewGuid(), name, row.Code, localityId, postalCodeId, address, phone);
    if (candidate.IsFailure)
    {
        invalidSchools++;
        Console.Error.WriteLine($"Skipped {row.Code}: {candidate.Error!.Code}");
        continue;
    }

    var wanted = candidate.Value;
    if (!schools.TryGetValue(row.Code, out var current))
    {
        db.Schools.Add(wanted);
        schools[row.Code] = wanted;
        newSchools++;
        if (++pending % ChunkSize == 0) await db.SaveChangesAsync();
        continue;
    }

    var changed = false;
    var entry = db.Entry(current);
    changed |= SetIfDifferent(entry, nameof(School.Name), current.Name, wanted.Name);
    changed |= SetIfDifferent(entry, nameof(School.NormalizedName), current.NormalizedName, wanted.NormalizedName);
    changed |= SetIfDifferent(entry, nameof(School.LocalityId), current.LocalityId, wanted.LocalityId);
    changed |= SetIfDifferent(entry, nameof(School.PostalCodeId), current.PostalCodeId, wanted.PostalCodeId);
    changed |= SetIfDifferent(entry, nameof(School.Address), current.Address, wanted.Address);
    changed |= SetIfDifferent(entry, nameof(School.Phone), current.Phone, wanted.Phone);
    if (changed) updatedSchools++;
}
await db.SaveChangesAsync();
await transaction.CommitAsync();

// ---------- 4. Report ----------
db.ChangeTracker.Clear();
Console.WriteLine();
Console.WriteLine($"Inserted this run: provinces {newProvinces}, localities {newLocalities}, postal codes {newPostalCodes}, schools {newSchools} | schools updated {updatedSchools}");
Console.WriteLine($"Dropped: invalid schools {invalidSchools}, phones longer than {MaxPhoneLength} chars (set to null) {phonesDropped}, addresses truncated to {MaxAddressLength} {addressesTruncated}");
Console.WriteLine(
    $"Database now: provinces {await db.Provinces.CountAsync()}, localities {await db.Localities.CountAsync()}, " +
    $"postal codes {await db.PostalCodes.CountAsync()}, schools {await db.Schools.CountAsync()} | " +
    $"schools without postal code {await db.Schools.CountAsync(s => s.PostalCodeId == null)}, " +
    $"without phone {await db.Schools.CountAsync(s => s.Phone == null)}");
return 0;

// ---------- helpers ----------
static string Cell(IDataReader reader, int index, int padTo = 0)
{
    if (index >= reader.FieldCount || reader.IsDBNull(index))
    {
        return string.Empty;
    }

    var value = reader.GetValue(index);
    var text = value is double number
        ? number.ToString("0", CultureInfo.InvariantCulture).PadLeft(padTo, '0')
        : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    return Collapse(text);
}

static string Collapse(string text) => Regex.Replace(text, @"\s+", " ").Trim();

static string FullName(SourceRow row) => Collapse($"{row.Generic} {row.Specific}");

static bool IsValidPostal(string code) => code.Length == PostalCode.Length && code.All(char.IsAsciiDigit) && code != "00000";

static void Count<TKey>(Dictionary<TKey, Dictionary<string, int>> spellings, TKey key, string spelling) where TKey : notnull
{
    if (!spellings.TryGetValue(key, out var counts))
    {
        counts = spellings[key] = new Dictionary<string, int>();
    }
    counts[spelling] = counts.GetValueOrDefault(spelling) + 1;
}

static string MostFrequent(Dictionary<string, int> spellings)
    => spellings.OrderByDescending(s => s.Value).ThenBy(s => s.Key, StringComparer.Ordinal).First().Key;

static bool SetIfDifferent<T>(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string property, T current, T wanted)
{
    if (EqualityComparer<T>.Default.Equals(current, wanted))
    {
        return false;
    }

    entry.Property(property).CurrentValue = wanted;
    return true;
}

static string ReadDevConnectionString()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
        {
            var file = Path.Combine(directory.FullName, "apps", "backend", "api", "appsettings.Development.json");
            if (!File.Exists(file)) continue;

            using var json = JsonDocument.Parse(File.ReadAllText(file));
            return json.RootElement.GetProperty("ConnectionStrings").GetProperty("SumiClasDb").GetString()!;
        }
    }

    throw new InvalidOperationException(
        "Connection string not found: pass it as the second argument or set SUMICLAS_DB.");
}

internal sealed record SourceRow(
    string Province,
    string Locality,
    string Generic,
    string Specific,
    string Code,
    string Address,
    string Postal,
    string Phone)
{
    public bool ConflictsWith(SourceRow other)
        => Differs(Province, other.Province) || Differs(Locality, other.Locality) || Differs(Generic, other.Generic)
           || Differs(Specific, other.Specific) || Differs(Address, other.Address) || Differs(Postal, other.Postal)
           || Differs(Phone, other.Phone);

    public SourceRow FillGapsFrom(SourceRow other) => this with
    {
        Province = Province.Length > 0 ? Province : other.Province,
        Locality = Locality.Length > 0 ? Locality : other.Locality,
        Generic = Generic.Length > 0 ? Generic : other.Generic,
        Specific = Specific.Length > 0 ? Specific : other.Specific,
        Address = Address.Length > 0 ? Address : other.Address,
        Postal = Postal.Length > 0 ? Postal : other.Postal,
        Phone = Phone.Length > 0 ? Phone : other.Phone,
    };

    private static bool Differs(string a, string b) => a.Length > 0 && b.Length > 0 && a != b;
}
