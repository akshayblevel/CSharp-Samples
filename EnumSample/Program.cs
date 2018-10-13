using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumSample
{
    class Program
    {
        enum OrderStatus
        {
            Pending = 0,
            Approved = 1,
            Shipped = 2,
            Delivered = 4,
            Cancelled = 8
        }

        static void Main(string[] args)
        {
            Console.WriteLine(OrderStatus.Approved); // Approved

            Console.WriteLine((int)OrderStatus.Shipped); //2

            Console.WriteLine(Enum.GetName(typeof(OrderStatus), 4)); //Delivered

            foreach (int i in Enum.GetValues(typeof(OrderStatus))) // 0 1 2 4 8
            {
                Console.WriteLine(i);
            }

            foreach (string str in Enum.GetNames(typeof(OrderStatus))) //Pending Approved Shipped Delivered Cancelled
            {
                Console.WriteLine(str);
            }

            OrderStatus os;

            // Initialize multiple flags using Bitwise OR
            os = OrderStatus.Shipped | OrderStatus.Cancelled; // 10
            Console.WriteLine(os);

            //Remove flag using Bitwise XOR
            os = os ^ OrderStatus.Shipped; // Cancelled
            Console.WriteLine(os);

            //Compare enum values
            Console.WriteLine((os & OrderStatus.Cancelled) == OrderStatus.Cancelled); // True

            Console.ReadLine();
        }
    }
}
