using TIPatcher.Interfaces;

namespace TIPatcher;
public class Program
{
    public static int Main(string[] args)
    {
        ConsoleLogger logger = new();
        Patcher patcher = new(logger);

        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i] == "-h" || args[i] == "/?" || args[i] == "--help"))
            {
                logger.Log(_help);
                return 0;
            }
            if ((args[i] == "-j" || args[i] == "--jar") && i + 1 < args.Length)
            {
                if (i + 1 < args.Length)
                {
                    patcher.PathToJar = args[i + 1];
                }
                else
                {
                    logger.Log(_invalidInput);
                    logger.Log(_help);
                    Wait(logger);
                    return -1;
                }
            }
            if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
            {
                if (i + 1 < args.Length)
                {
                    patcher.OutputDir = args[i + 1];
                }
                else
                {
                    logger.Log(_invalidInput);
                    logger.Log(_help);
                    Wait(logger);
                    return -1;
                }
            }
        }

        patcher.Patch();
        Wait(logger);
        return 0;
    }

    private static void Wait(ILogger logger)
    {
        logger.Log("Press any key to exit...");
        logger.WaitForInput();
    }
    private static string _invalidInput => "invalid input!";
    private static string _help =>
            """
            TIPatcher

            Note: 
            You need the TI-Nspire CX CAS Student Software installed.

            Commands:
            -o, --output <OUTPUTDIR>    | Outputs patched jar to OUTPUTDIR. 
                                          If not specified, OUTPUTDIR is the running directory.
            -j, --jar <PATHTODOCFW.JAR> | Uses docfw.jar at path. If this argument isn't specified, 
                                          the patcher uses the docfw.jar from the TI-Nspire CX Cas Student
                                          Software installation
            """;
    
}