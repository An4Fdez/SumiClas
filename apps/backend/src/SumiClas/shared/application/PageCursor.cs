using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace SumiClas.Shared.Application;

/// <summary>Opaque keyset cursor: the sort key (as a number) and the id of the last item of the previous page.</summary>
public sealed record PageCursor(long Key, Guid Id)
{
    public string Encode()
        => Base64Url.EncodeToString(Encoding.UTF8.GetBytes($"{Key.ToString(CultureInfo.InvariantCulture)}|{Id:D}"));

    public static bool TryDecode(string? value, out PageCursor cursor)
    {
        cursor = null!;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        try
        {
            string[] parts = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(value)).Split('|');
            if (parts.Length != 2
                || !long.TryParse(parts[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long key)
                || !Guid.TryParseExact(parts[1], "D", out Guid id))
            {
                return false;
            }

            cursor = new PageCursor(key, id);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
