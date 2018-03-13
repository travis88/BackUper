using BackUp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUp.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigParams _params = new ConfigParams();
            Launcher.Start(_params);
        }
    }
}
