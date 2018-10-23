using System;
using System.Runtime.CompilerServices;

namespace CallerInfoAttribute
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Main method Start");
            InsertInDB();
            BusinessLogic();
            Console.WriteLine("Main method End!");
            Console.ReadLine();
        }

        static void BusinessLogic()
        {
            InsertInDB();
        }

        static void InsertInDB([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string message = string.Format("InsertInDB method is called by {0} from line number {1} at {2}", memberName, sourceLineNumber, DateTime.Now.ToString());
            Log(message);
        }

        static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
