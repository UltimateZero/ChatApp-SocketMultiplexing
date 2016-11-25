using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    
    class Program
    {
        
        static void Main(string[] args)
        {
            int[] nums = { 1, 2, 3, 4, 5, 6 };

            var lessThan = from num in nums where num < 4 select num;

            foreach(int num in lessThan)
            {
                Console.WriteLine(num);
            }
        }
    }
}
