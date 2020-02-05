using System;

namespace SvenskaInstruktioner
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { "test_code.txt" };

            if (args.Length > 0)
            {
                new Interpreter(args[0]).Interpret();
            }
        }
    }
}
