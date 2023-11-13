using System.Globalization;

namespace Backend.Core.Validations
{
    public abstract class RequestValidation
    {
        public bool InvalidLength(string value, int maxLength) => string.IsNullOrWhiteSpace(value) || value.Length > maxLength;

        public bool InvalidDate(string value, string format, out DateOnly date)
        {
            date = DateOnly.MinValue;

            if (InvalidLength(value, 10))
                return true;

            if (DateOnly.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly data))
            {
                date = data;
                return false;
            }

            return true;
        }

        public abstract bool Validate();
    }
}
