// See https://aka.ms/new-console-template for more information

using Bam.CommandLine;

public class Program: CommandLineTool
{
    static void Main(string[] args)
    {
        ExecuteMainOrInteractive(args);
    }
}
