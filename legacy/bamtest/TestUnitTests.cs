using System;
using Bam.Console;
using Bam.Test;

namespace Bam.Net.Testing
{
    [Serializable]
    public class TestUnitTests: UnitTestMenuContainer
    {
        [UnitTest]
        public void PassingTest()
        {
            Expect.IsTrue(true);
            Message.PrintLine("Passing test should pass");
        }

        [UnitTest]
        public void FailingTest()
        {
            Expect.IsTrue(false);            
        }
    }
}
