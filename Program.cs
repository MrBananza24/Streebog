using System.Numerics;
using System.Text;

namespace IS_Lab_Streebog
{
    class Program
    {
        static void Main()
        {
            //string input = "FBE2E5F0EEE3C820FBEAFAEBEF20FFFBF0E1E0F0F520E0ED20E8ECE0EBE5F0F2F120FFF0EEEC20F120FAF2FEE5E2202CE8F6F3EDE220E8E6EEE1E8F0F2D1202CE8F0F2E5E220E5D1";

            Console.WriteLine("Введите строку пожалуйста:");
            string input = Console.ReadLine() ?? string.Empty;

            //byte[] m = Convert.FromHexString(input);
            byte[] m = Encoding.UTF8.GetBytes(input);

            Console.WriteLine($"Streebog512: {Convert.ToHexString(Streebog.Hash(m))}");
            Console.WriteLine($"Streebog256: {Convert.ToHexString(Streebog.Hash(m, false))}");
        }
    }
}