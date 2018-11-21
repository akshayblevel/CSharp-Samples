using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading_ThreadStatic
{
    class Program
    {
        // A thread has its own call stack that stores all the methods that are executed. Local variables are stored on the call stack 
        // and are private to the thread.
        // A thread can also have its own data that’s not a local variable.By marking a field with the ThreadStatic attribute, 
        // each thread gets its own copy of a field

        [ThreadStatic]
        public static int localVariable;
        public static void Main()
        {
            new Thread(() =>
            {
                for (int x = 0; x < 10; x++)
                {
                    localVariable++;
                    Console.WriteLine("Thread 1: {0}", localVariable);
                }
            }).Start();

            new Thread(() =>
            {
                for (int x = 0; x < 10; x++)
                {
                    localVariable++;
                    Console.WriteLine("Thread 2: {0}", localVariable);
                }
            }).Start();

            Console.ReadKey();
        }

        // With the ThreadStaticAttribute applied, the maximum value of localVariable becomes 10. If you remove it, 
        //you can see that both threads access the same value and it becomes 20.

        //class Program
        //{
        //    public static int localVariable;
        //    public static void Main()
        //    {
        //        new Thread(() =>
        //        {
        //            for (int x = 0; x < 10; x++)
        //            {
        //                localVariable++;
        //                Console.WriteLine("Thread 1: {0}", localVariable);
        //            }
        //        }).Start();

        //        new Thread(() =>
        //        {
        //            for (int x = 0; x < 10; x++)
        //            {
        //                localVariable++;
        //                Console.WriteLine("Thread 2: {0}", localVariable);
        //            }
        //        }).Start();

        //        Console.ReadKey();
        //    }
        //}
    }
}
