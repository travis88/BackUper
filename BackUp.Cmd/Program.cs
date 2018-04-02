using BackUp.Core;
using System;

namespace BackUp.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("creater backup's");
            Console.WriteLine("press any button to run");
            ConfigParams _params = new ConfigParams();
            Launcher.Start(_params);
            Console.WriteLine("backup's done");
            Console.ReadLine();
        }
    }
}
