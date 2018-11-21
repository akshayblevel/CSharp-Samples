using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading_StopThread
{
    class Program
    {
        //	We can use Thread.Abort method to stop the thread. As Thread.Abort method is executed by another thread which can happen at anytime 
        // and sometime results into ThreadAbortException. So it's better to stop thread by using a shared variable which is accessible by target 
        // and calling thread.

        static void Main(string[] args)
        {
            bool isStop = false;

            Thread t = new Thread(new ThreadStart(() =>
            {
                while (!isStop)
                {
                    Console.WriteLine("Running...");
                    Thread.Sleep(1000);
                }
            }));

            t.Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            isStop = true;
            t.Join();

        }
    }
}
