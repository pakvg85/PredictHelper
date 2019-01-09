using System;
using System.Collections.Generic;
using System.Linq;

namespace PredictHelper
{
    public static partial class CommonExtensions
    {
        public static IEnumerable<SideGroup> GetEnumTypes => Enum.GetValues(typeof(SideGroup)).Cast<SideGroup>();
    }

    public enum SideGroup
    {
        Undefined = 0,
        Claimant,
        Defendant,
        ThirdParty
    }
}