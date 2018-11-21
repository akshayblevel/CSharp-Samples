using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading_ThreadClass
{
    class Program
    {
        static void Main(string[] args)
        {
            // Thread class is found under system.threading namespace. 
            // Thread.join method is called to wait until the other thread finishes.

            Thread t = new Thread(new ThreadStart(SubMethod));
            t.Start();
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine("Main thread: Do some work.");
                Thread.Sleep(100);
            }
            t.Join();

            Console.ReadKey();
        }

        public static void SubMethod()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("SubMethod Loop: {0}", i);
                Thread.Sleep(100);
            }
        }
    }
}
