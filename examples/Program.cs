using System;
using System.Threading.Tasks;

namespace CircularEnterpriseApis.Examples
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Circular Protocol C# Enterprise APIs - Examples");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (args.Length > 0 && args[0] == "chain")
            {
                await SimpleCertificateSubmission.RunChainSubmissionExample();
            }
            else
            {
                await SimpleCertificateSubmission.RunExample();
            }

            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run                    - Run simple certificate submission");
            Console.WriteLine("  dotnet run chain              - Run chain submission example");
            Console.WriteLine();
            Console.WriteLine("Required environment variables:");
            Console.WriteLine("  CIRCULAR_PRIVATE_KEY         - Your private key (hex format)");
            Console.WriteLine("  CIRCULAR_ADDRESS             - Your wallet address (hex format)");
        }
    }
}