using System.Collections.Generic;

namespace Fast.Api
{
    public class OptionModel
    {
        public bool IsAlone { get; set; }

        public bool IsResource { get; set; }

        public List<string> FilterUrl { get; set; } = new List<string>();

        public string dbFile { get; set; } = "db.json";
    }
}
