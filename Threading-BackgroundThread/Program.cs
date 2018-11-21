using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading_BackgroundThread
{
    class Program
    {
        //	Application exits immediately

        public static void SubMethod()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("SubMethod Loop: {0}", i);
                Thread.Sleep(100);
            }
        }
        static void Main(string[] args)
        {

            Thread t = new Thread(new ThreadStart(SubMethod));
            t.IsBackground = true;
            t.Start();

            //Console.ReadKey();
        }

    }
}
