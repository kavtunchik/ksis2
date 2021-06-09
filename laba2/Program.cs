using System;
using System.Net;
using System.Net.Sockets;


namespace CNaS_Lab_2
{
    public class Program
    {
        static void Main(string[] args)
        {
            Tracert tracert = new Tracert();

            if (args.Length == 1)
            {
                tracert.Start(args[0]);
            }
            else if (args.Length > 1)
            {
                tracert.Start(args[0], args[1].Equals("-d"));

            }
            
        }
    }
}