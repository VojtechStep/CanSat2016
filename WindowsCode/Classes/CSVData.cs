using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WindowsCode.Classes
{
    public class CSVData
    {
        public CSVData(String input)
        {
            String[] parts = input.Split(',');

            if (parts.Length == 11)
            {
                try
                {
                    UTCTime = Single.Parse(parts[0]);
                    Temperature = Single.Parse(parts[1]);
                    Pressure = Single.Parse(parts[2]);
                    X_Acceleration = Single.Parse(parts[3]);
                    Z_Acceleration = Single.Parse(parts[5]);
                    Y_Acceleration = Single.Parse(parts[4]);
                    Latitude = Single.Parse(parts[6]);
                    if (parts[7].Count() > 0)
                        LatitudeDirection = (Direction)parts[7].ElementAt(0);
                    else
                        LatitudeDirection = Direction.UNDEFINED;
                    Longitude = Single.Parse(parts[8]);
                    if (parts[9].Count() > 0)
                        LatitudeDirection = (Direction)parts[9].ElementAt(0);
                    else
                        LatitudeDirection = Direction.UNDEFINED;
                    Altitude = Single.Parse(parts[10]);
                    RawData = input;
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Something went wrong during the parsing of the CSV input value ({input})");
                }
            }

        }

        public Single UTCTime { get; private set; }
        public Single Temperature { get; private set; }
        public Single Pressure { get; private set; }
        public Double X_Acceleration { get; private set; }
        public Double Y_Acceleration { get; private set; }
        public Double Z_Acceleration { get; private set; }
        public Single Latitude { get; private set; }
        public Direction LatitudeDirection { get; private set; }
        public Single Longitude { get; private set; }
        public Direction LongitudeDirection { get; private set; }
        public Single Altitude { get; private set; }
        public String RawData { get; private set; }
        public Double LatitudeInDegrees
        {
            get
            {
                if (Latitude > Math.Pow(10, 5))
                {
                    Double Minutes = Double.Parse(Latitude.ToString().Substring(Latitude.ToString().Length - 5));
                    return (Latitude - Minutes) / (Math.Pow(10, Latitude.ToString().Length - 5)) + (Minutes / 60);
                } return 0;
            }
        }
        public Double LongitudeInDegrees
        {
            get
            {
                if (Longitude > Math.Pow(10, 5))
                {
                    Double Minutes = Double.Parse(Longitude.ToString().Substring(Longitude.ToString().Length - 5));
                    return (Longitude - Minutes) / (Math.Pow(10, Longitude.ToString().Length - 5)) + (Minutes / 60);
                } return 0;
            }
        }
    }

    public enum Direction
    {
        NORTH = 'N',
        SOUTH = 'S',
        EAST = 'E',
        WEST = 'W',
        UNDEFINED
    }

}
