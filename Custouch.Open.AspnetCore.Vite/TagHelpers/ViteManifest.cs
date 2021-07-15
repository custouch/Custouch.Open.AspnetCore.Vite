using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Custouch.Open.AspnetCore.Vite.TagHelpers
{
    public class ViteManifest:Dictionary<string, ViteManifestItem>
    {

    }
    public class ViteManifestItem
    {
        public string File { get; set; }
        public string Src { get; set;  }
        public bool IsEntry { get; set;  }
        public string[] DynamicImports { get; set; }
        public string[] Css { get; set; }
        public string[] Assets { get; set; }
    }
}
