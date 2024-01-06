using Bam.CommandLine;
using Bam.Console;
using Bam.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BamTest
{
    public class ConsoleActions : CommandLineTool
    {
        [ConsoleAction]
        public void Test()
        {
            Message.PrintLine("hello from the console");
            MethodInfo methodInfo = typeof(ConsoleActions).GetMethod("Test");
            //Message.PrintLine(methodInfo.Full)
        }

        [UnitTest]
        public void Test2()
        {
            Message.PrintLine("hello");
        }
    }
}
