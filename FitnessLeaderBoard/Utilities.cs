using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard
{
    public static class Utilities
    {
        public static string Truncate(this string input, int numberOfCharacters)
        {
            var inputLength = input.Length;

            if (inputLength <= numberOfCharacters)
                return input;
            else
            {
                var result = input.Substring(0, numberOfCharacters - 3) + "...";
                return result;
            }
        }
    }
}
