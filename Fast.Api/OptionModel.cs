using System.Collections.Generic;

namespace Fast.Api
{
    public class OptionModel
    {
        public bool IsAlone { get; set; }

        public List<string> FilterUrl { get; set; } = new List<string>();
    }
}
