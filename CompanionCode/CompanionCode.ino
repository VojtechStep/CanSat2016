/*
 Name:		CompanionCode.ino
 Created:	3/9/2016 9:50:59 AM
 Author:	Adalbert
*/

#include <RFM69registers.h>
#include <RFM69.h>
#include <qbcan.h>
#include <BMP180.h>
#include <SD.h>

/*
	Serial - Debug/Probe -- 115200
*/

const byte broadcastInitMessage = 0x04;
const byte mesurementStartMessage = 0x05;
const byte mesurementPauseMessage = 0x06;
const byte mesurementEndMessage = 0x07;
const byte packetStartMessage = 0x08;
const byte packetEndMessage = 0x09;
const byte takeAndSaveImageMessage = 0x0A;

File logFile;
byte inByte;

void setup()
{
	Serial.begin(115200);
	if (!SD.begin())
	{
		return;
	}
	
}

// the loop function runs over and over again until power down or reset
void loop()
{
	if (Serial.available())
	{
		inByte = Serial.read();
		if (inByte == packetStartMessage)
		{
			logFile = SD.open("data.log", FILE_WRITE);
			while (Serial.available())
			{
				logFile.write(Serial.read());
			}
			logFile.close();
		}
	}
}
