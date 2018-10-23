using System;
using System.Text.RegularExpressions;

namespace PredicateDelegate
{
    public class Program
    {
        public static void Main()
        {
            Predicate<string> validateMobile = IsValidMobile;
            bool result = validateMobile("9999999999");

            Console.WriteLine(result);

            //PREDICATE WITH LAMBDA EXPRESSION
            Predicate<string> validateMobile1 = (mobile) =>
            {
                Regex r = new Regex(@"^[0-9]{10}$");
                return r.IsMatch(mobile);
            };

            bool result1 = validateMobile1("888888888");
            Console.WriteLine(result1);

            Console.ReadLine();
        }

        public static bool IsValidMobile(string mobile)
        {
            Regex r = new Regex(@"^[0-9]{10}$");
            return r.IsMatch(mobile);
        }
    }
}
