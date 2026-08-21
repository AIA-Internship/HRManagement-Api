namespace HRManagement.MsSQL.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizePhoneNumber(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string cleanNumber = input.Replace(" ", "").Replace("-", "").Trim();

            // 2. Cek Prefix dan sesuaikan
            if (cleanNumber.StartsWith("+62")) return cleanNumber.Substring(3);
            if (cleanNumber.StartsWith("62")) return cleanNumber.Substring(2);
            if (cleanNumber.StartsWith("0")) return cleanNumber.Substring(1);

            // Jika input sudah 8xxx, kembalikan apa adanya
            return cleanNumber;
        }
    }
}
