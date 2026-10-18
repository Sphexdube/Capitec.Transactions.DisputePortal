using System.Globalization;
using System.Text;

namespace Capitec.Transactions.DisputePortal.Application.Paging
{
    public static class PageCursorCodec
    {
        private const char Separator = ':';

        public static string Encode(PageCursor cursor)
        {
            ArgumentNullException.ThrowIfNull(cursor);

            string payload = string.Create(CultureInfo.InvariantCulture, $"{cursor.Position.UtcTicks}{Separator}{cursor.Id}");
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static PageCursor? Decode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > PageLimits.CursorMaximumLength)
            {
                return null;
            }

            string normalized = value.Replace('-', '+').Replace('_', '/');
            int paddingLength = normalized.Length % 4;

            if (paddingLength == 1)
            {
                return null;
            }

            normalized = normalized.PadRight(normalized.Length + ((4 - paddingLength) % 4), '=');

            try
            {
                string payload = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
                string[] values = payload.Split(Separator);

                if (values.Length != 2
                    || !long.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out long utcTicks)
                    || !long.TryParse(values[1], NumberStyles.None, CultureInfo.InvariantCulture, out long id)
                    || utcTicks > DateTimeOffset.MaxValue.UtcTicks
                    || id <= 0)
                {
                    return null;
                }

                return new PageCursor(new DateTimeOffset(utcTicks, TimeSpan.Zero), id);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static bool IsValidOrEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value) || Decode(value) is not null;
        }
    }
}
