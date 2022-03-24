using System.Collections.Generic;

namespace NumberConverter
{
    public class BinDec
    {
        public static List<int> DecToBin(int number)
        {
            List<int> bin = new List<int>();
            for (int i = 0; number > 0; i++)
            {
                bin.Add(number % 2);
                number /= 2;
            }
            bin.Reverse();
            return bin;
        }

        public static int BinToDec(List<int> bin)
        {
            int number = 0;
            int x = 1;
            bin.Reverse();
            foreach (int bit in bin)
            {
                if (bit == 1)
                {
                    number += x;
                }
                x *= 2;
            }
            return number;
        }
    }
}
