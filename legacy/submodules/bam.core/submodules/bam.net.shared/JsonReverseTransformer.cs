﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net
{
    public class JsonReverseTransformer<TUntransformed> : IValueReverseTransformer<string, TUntransformed>
    {
        public TUntransformed ReverseTransform(string encoded)
        {
            return encoded.FromJson<TUntransformed>();
        }

        public IValueTransformer<TUntransformed, string> GetTransformer()
        {
            return (IValueTransformer<TUntransformed, string>)new JsonTransformer<string>();
        }
    }
}
