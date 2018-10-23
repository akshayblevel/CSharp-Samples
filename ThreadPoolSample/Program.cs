using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolSample
{

    class Program
    {
        static int counter = 0;
        static void Main(string[] args)
        {
            AutoResetEvent mainEvent = new AutoResetEvent(false);
            int workerThreads;
            int portThreads;

            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            Console.WriteLine("\nMaximum worker threads: \t{0}" + "\nMaximum completion port threads: {1}", workerThreads, portThreads);
            ThreadPool.GetMinThreads(out workerThreads, out portThreads);
            Console.WriteLine("\nMaximum worker threads: \t{0}" + "\nMaximum completion port threads: {1}", workerThreads, portThreads);

            ThreadPool.SetMaxThreads(500, 500);

            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            Console.WriteLine("\nMaximum worker threads: \t{0}" + "\nMaximum completion port threads: {1}", workerThreads, portThreads);
            ThreadPool.GetMinThreads(out workerThreads, out portThreads);
            Console.WriteLine("\nMaximum worker threads: \t{0}" + "\nMaximum completion port threads: {1}", workerThreads, portThreads);

            Console.ReadLine();

            for (int i = 0; i < 1000; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), mainEvent);

                ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
                Console.WriteLine("\nAvailable worker threads: \t{0}" + "\nAvailable completion port threads: {1}\n", workerThreads, portThreads);

            }

            Console.ReadLine();
        }

        public static void DoWork(object mainEvent)
        {
            counter += 1;
            Console.WriteLine("hi " + counter.ToString());
            Thread.Sleep(10000);

            ((AutoResetEvent)mainEvent).Set();
        }
    }
}
