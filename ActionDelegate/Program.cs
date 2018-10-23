using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionDelegate
{
    public class Program
    {
        public static void Main()
        {

            Action<string, double, string> rechargeAction = Recharge;
            rechargeAction("9999999999", 100, "Vodafone");

            // ACTION WITH LAMBDA EXPRESSION
            Action<string, double, string> rechargeAction1 = (mobile, amount, mobileOperator) => {
                //Recharge will be done and successful message will be sent through SMS
                //Nothing will be returned from this method.
                Console.WriteLine(string.Format("Recharge is sucessfully done. Mobile: {0} Amount:{1} Operator:{2}", mobile, amount, mobileOperator));
            };
            rechargeAction1("8888888888", 200, "Idea");

            Console.ReadLine();
        }

        public static void Recharge(string mobile, double amount, string mobileOperator)
        {
            //Recharge will be done and successful message will be sent through SMS
            //Nothing will be returned from this method.
            Console.WriteLine(string.Format("Recharge is sucessfully done. Mobile: {0} Amount:{1} Operator:{2}", mobile, amount, mobileOperator));

        }
    }
}
