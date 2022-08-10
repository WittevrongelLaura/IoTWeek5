using System;
using System.Collections.Generic;
using System.Text;

namespace IoTWeek5.models
{
    public class Registration
    {
        public string GarbageRegistrationId { get; set; }
        public string Name { get; set; }
        public string EMail { get; set; }
        public string Description { get; set; }
        public string GarbageTypeId { get; set; }
        public string CityId { get; set; }
        public string Street { get; set; }
        public float Weight { get; set; }
        public float Lat { get; set; }
        public float Long { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
