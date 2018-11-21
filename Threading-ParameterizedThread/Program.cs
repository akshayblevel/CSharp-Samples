using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading_ParameterizedThread
{
    class Program
    {
        //	When we want to pass argument in a method which executes on thread.

        public static void SubMethod(object count)
        {
            for (int i = 0; i < (int)count; i++)
            {
                Console.WriteLine("SubMethod Loop: {0}", i);
                Thread.Sleep(100);
            }
        }
        static void Main(string[] args)
        {

            Thread t = new Thread(new ParameterizedThreadStart(SubMethod));
            t.Start(5);
            t.Join();
            Console.ReadKey();
        }

    }
}
