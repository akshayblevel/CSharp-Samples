using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading_ThreadLocal
{
    class Program
    {
        //	When we want to use local data in thread and initialize it for each thread, we can use ThreadLocal class.

        public static ThreadLocal<int> localVariable =
             new ThreadLocal<int>(() =>
             {
                 return Thread.CurrentThread.ManagedThreadId;
             });

        public static void Main()
        {
            new Thread(() =>
            {
                for (int x = 0; x < localVariable.Value; x++)
                {
                    Console.WriteLine("Thread 1: {0}", x);
                }
            }).Start();

            new Thread(() =>
            {
                for (int x = 0; x < localVariable.Value; x++)
                {
                    Console.WriteLine("Thread 2: {0}", x);
                }
            }).Start();
            Console.ReadKey();
        }

    }
}
