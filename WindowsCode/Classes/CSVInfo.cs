using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WindowsCode.Classes
{
    public class CSVInfo
    {
        public CSVInfo(String input)
        {
            String[] parts = input.Split(',');

            if (parts.Length == 10)
            {
                try
                {
                    Temperature = Double.Parse(parts[0]);
                    Pressure = Double.Parse(parts[1]);
                    X_Acceleration = Double.Parse(parts[2]);
                    Y_Acceleration = Double.Parse(parts[3]);
                    Z_Acceleration = Double.Parse(parts[4]);
                    GPS_OK = parts[5].ToCharArray()[0] == 'A';
                    Latitude = Double.Parse(parts[6]);
                    LatitudeDirection = (Direction)parts[7].ToCharArray()[0];
                    Longitude = Double.Parse(parts[8]);
                    LongitudeDirection = (Direction)parts[9].ToCharArray()[0];
                    RawData = input;
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Something went wrong during the parsing of the CSV input value ({input})");
                }
            }

        }

        public Double Temperature { get; private set; }
        public Double Pressure { get; private set; }
        public Double X_Acceleration { get; private set; }
        public Double Y_Acceleration { get; private set; }
        public Double Z_Acceleration { get; private set; }
        public Boolean GPS_OK { get; private set; }
        public Double Latitude { get; private set; }
        public Direction LatitudeDirection { get; private set; }
        public Double Longitude { get; private set; }
        public Direction LongitudeDirection { get; private set; }
        public String RawData { get; private set; }
    }

    public enum Direction
    {
        NORTH = 'N',
        SOUTH = 'S',
        EAST = 'E',
        WEST = 'W'
    }


}
