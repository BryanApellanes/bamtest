﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net
{
    public class Base64Transformer : ValueTransformer<byte[], string>
    {
        public override byte[] ReverseTransform(string output)
        {
            return output.FromBase64();
        }

        public override string Transform(byte[] input)
        {
            return input.ToBase64();
        }

        public override IValueReverseTransformer<string, byte[]> GetReverseTransformer()
        {
            return new Base64ReverseTransformer();
        }
    }
}
