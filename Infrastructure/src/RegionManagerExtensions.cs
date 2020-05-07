using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Regions;

namespace Infrastructure.src
{
    public static class RegionManagerExtensions
    {
        public static void RemoveActiveView(this IRegion region)
        {
            region.Remove(region.ActiveViews.First());
        }
    }
}
