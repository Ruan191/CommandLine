using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JumpSquareGames.CommandLine
{
    public static class CommandLineExtentions
    {
        public static bool IsNumber(this string text)
        {
            bool isNumber = true;

            foreach (var letter in text)
            {
                if (letter == '.')
                    continue;

                if ((int)letter < 48 || (int)letter > 57)
                {
                    isNumber = false;
                    break;
                }
            }

            return isNumber;
        }
    } 
}
