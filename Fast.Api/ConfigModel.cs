﻿namespace Fast.Api
{
    public class ConfigModel
    {
        public bool IsResource { get; set; }

        public string dbKey { get; set; }

        public string dbFile { get; set; } = "db.json";

        public string mapFile { get; set; } = "map.json";
    }
}