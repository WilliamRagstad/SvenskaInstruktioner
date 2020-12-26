using SvenskaInstruktioner.Core;
using SvenskaInstruktioner.Helper;
using System;
using System.IO;

namespace SvenskaInstruktioner
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { "../../../../Language Mockups/test_code.txt" };

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (File.Exists(args[i]))
                    {
                        ExitFlag f = new Interpreter(args[i]).Interpret(true);
                        Console.WriteLine(); // Ny Rad
                        switch(f)
                        {
                            case ExitFlag.Successfull:
                                Functions.WriteLineColor($"[KLAR] Programmet kördes utan problem!", ConsoleColor.Green);
                                break;
                            case ExitFlag.SyntaxError:
                                Functions.WriteLineColor($"[KLAR] Programmet var inte komplett när det kördes.", ConsoleColor.Red);
                                break;
                            case ExitFlag.FatalError:
                                Functions.WriteLineColor($"[KLAR] Programmet krashade på grund av ett alvarligt fel!", ConsoleColor.Red);
                                break;
                            default:
                                Functions.WriteLineColor($"[KLAR] Programmet krashade på grund av ett okänt fel!", ConsoleColor.Red);
                                break;
                        }
                    }
                    else
                        Functions.WriteLineColor($"Filen '{args[i]}' kunde inte hittas!", ConsoleColor.Red);
                }
            }
            else
                Functions.WriteLineColor("Inga filer tillhandahölls.", ConsoleColor.Red);

            Console.ReadKey(true);
        }
    }
}
