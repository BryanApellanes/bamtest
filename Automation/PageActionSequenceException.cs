using System;

namespace Bam.Net.Testing.Automation
{
    public class PageActionSequenceException: Exception
    {
        public PageActionSequenceException(PageActionSequence pageActionSequence, string message) : base(message)
        {
            PageActionSequence = pageActionSequence;
        }

        public PageActionSequence PageActionSequence { get; set; }
    }
}
