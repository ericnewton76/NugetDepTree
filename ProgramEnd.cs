using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetDepTree
{
    public class ProgramEnd
    {

        public static void Wait(int seconds)
        {
            if (Console.IsOutputRedirected || Console.IsInputRedirected) return;

            Task.Run(() => {
                Console.WriteLine("Program ended.");
                Console.ReadKey();
            }).Wait(seconds * 1000);
        }

    }
}
