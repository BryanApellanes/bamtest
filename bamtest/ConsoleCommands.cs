using Bam.Console;
using Bam.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bam.CoreServices;
using Bam.Shell;

namespace BamTest
{
    [ConsoleMenu("bamtest options")]
    public class ConsoleCommands : ConsoleMenuContainer
    {
        // TODO: refer to CommandLineTestTool for implementation inspiration
        
        public ConsoleCommands(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [ConsoleCommand("Run all unit tests", "Run all unit tests")]
        [MenuItem("Run All Unit Tests", "Run all unit tests")]
        public void RunAllUnitTests()
        {
            throw new NotImplementedException();
        }

        [ConsoleCommand("Run unit test")]
        [MenuItem("Run Unit Test")]
        public void RunUnitTest()
        {
            throw new NotImplementedException();
        }

        [ConsoleCommand("Run all spec tests", "Run all spec tests")]
        [MenuItem("Run All Spec Tests", "Run all spec tests")]
        public void RunAllSpecTests()
        {
            throw new NotImplementedException();
        }
        
        [ConsoleCommand("Run spec test", "Run spec test")]
        [MenuItem("Run Spec Test", "Run all spec test")]
        public void RunSpecTest()
        {
            throw new NotImplementedException();
        }

        [ConsoleCommand("Run all integration tests", "Run all integration tests")]
        [MenuItem("Run All Integration Tests", "Run all integration tests")]
        public void RunAllIntegrationTests()
        {
            throw new NotImplementedException();
        }
        
        [ConsoleCommand("Run integration test", "Run integration test")]
        [MenuItem("Run integration Test", "Run all integration test")]
        public void RunIntegrationTest()
        {
            throw new NotImplementedException();
        }
    }
}
