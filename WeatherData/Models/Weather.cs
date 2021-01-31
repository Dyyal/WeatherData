using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherData.Models
{
    class Weather
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
