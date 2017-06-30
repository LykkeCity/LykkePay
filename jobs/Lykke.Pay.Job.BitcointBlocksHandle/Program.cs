using System;
using System.Net;
using Bitnet.Client;

namespace Lykke.Pay.Job.BitcointBlocksHandle
{
    class Program
    {
        static void Main(string[] args)
        {
            var bc = new BitnetClient("http://127.0.0.1:8332");
            bc.Credentials = new NetworkCredential("user", "pass");
            var p = bc.GetDifficulty();
            Console.WriteLine(p);
        }
    }
}