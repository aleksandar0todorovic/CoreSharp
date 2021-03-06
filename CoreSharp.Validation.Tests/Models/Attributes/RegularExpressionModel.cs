﻿using CoreSharp.Common.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    public class RegularExpressionModel
    {
        [RegularExpression("[d]+")]
        public string Name { get; set; }

        [RegularExpression("[d]+", IncludePropertyName = true)]
        public string Name2 { get; set; }
    }
}
