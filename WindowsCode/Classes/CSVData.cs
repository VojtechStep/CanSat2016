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
                    X_Acceleration = Single.Parse(parts[3]) * SettingsState.GRange / 1024;
                    Y_Acceleration = Single.Parse(parts[4]) * SettingsState.GRange / 1024;
                    Z_Acceleration = Single.Parse(parts[5]) * SettingsState.GRange / 1024;
                    Latitude = Single.Parse(parts[6]);
                    LatitudeDirection = (Direction)parts[7].ToCharArray()[0];
                    Longitude = Single.Parse(parts[8]);
                    LongitudeDirection = (Direction)parts[9].ToCharArray()[0];
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
        public Single X_Acceleration { get; private set; }
        public Single Y_Acceleration { get; private set; }
        public Single Z_Acceleration { get; private set; }
        public Single Latitude { get; private set; }
        public Direction LatitudeDirection { get; private set; }
        public Single Longitude { get; private set; }
        public Direction LongitudeDirection { get; private set; }
        public Single Altitude { get; private set; }
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
