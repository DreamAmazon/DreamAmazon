using System;

namespace DreamAmazon
{
    public static class Contracts
    {
        public static void Require(bool b)
        {
            if (!b)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}