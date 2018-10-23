using System;

namespace FuncDelegate
{
    class Program
    {
        public static void Main()
        {
            Func<string, double, string, string> func = Recharge;

            string result = func("9999999999", 100, "Vodafone");
            Console.WriteLine(result);

            //FUNC WITH ANONYMOUS FUNCTION
            Func<string, double, string, string> func1 = delegate (string mobile, double amount, string mobileOperator)
            {
                string str = string.Format("Recharge is sucessfully done. Mobile: {0} Amount:{1} Operator:{2}", mobile, amount, mobileOperator);
                return str;
            };

            string result1 = func1("8888888888", 200, "Idea");
            Console.WriteLine(result1);

            //FUNC WITH LAMBDA EXPRESSION
            Func<string, double, string, string> func2 = (mobile, amount, mobileOperator) => string.Format("Recharge is sucessfully done. Mobile: {0} Amount:{1} Operator:{2}", mobile, amount, mobileOperator);

            string result2 = func2("7777777777", 300, "JIO");
            Console.WriteLine(result2);

            Console.ReadLine();
        }

        public static string Recharge(string mobile, double amount, string mobileOperator)
        {
            string str = string.Format("Recharge is sucessfully done. Mobile: {0} Amount:{1} Operator:{2}", mobile, amount, mobileOperator);
            return str;
        }
    }
}
