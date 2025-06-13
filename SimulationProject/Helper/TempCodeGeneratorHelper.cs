using System.Security.Cryptography;
using System.Text;

namespace SimulationProject.Helper
{
    public class TempCodeGeneratorHelper
    {
        private const string RandomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";

        public static string GenerateCode(int length)
        {
            string tmpCode = string.Empty;
            if (length > 0)
            {
                var code = new StringBuilder(length);
                using var rng = RandomNumberGenerator.Create();
                var buffer = new byte[sizeof(uint)];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    uint num = BitConverter.ToUInt32(buffer, 0);
                    code.Append(RandomChars[(int)(num % RandomChars.Length)]);
                }
                tmpCode = code.ToString();
            }
            return tmpCode;
        }
    }
}
