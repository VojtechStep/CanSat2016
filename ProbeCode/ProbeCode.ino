/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch && Jacob (Vojtěch is first, because he actually works)

 SD Write speed: 1Mb / 48508mS = 0.0206 Mb/S

 Serial: PC
 Serial1: GPS
 Serial2: Camera subsystem

 Packet Structure:
 **|Type|**|Name|******|Unit|*********|Data Decomposition|*****************************************************|Length|*|Comment|************
 *  Byte	PacketStart	[]			= 0x08																		1b							*
 *	Single	UTCTime		[hhmmss.sss]= (data[ 1] << 24) | (data[ 2] << 16) | (data[ 3] << 8) | (data[ 4] << 0)	4b							*
 *	Single	Temperature [°C]		= (data[ 5] << 24) | (data[ 6] << 16) | (data[ 7] << 8) | (data[ 8] << 0)	4b							*
 *	Single	Pressure	[mB]		= (data[ 9] << 24) | (data[10] << 16) | (data[11] << 8) | (data[12] << 0)	4b							*
 *	Short	XAccRaw		[1]			=										(data[13] << 8) | (data[14] << 0)	2b		xg = x * Grange/1024*
 *	Short	YAccRaw		[1]			=										(data[15] << 8) | (data[16] << 0)	2b		yg = y * Grange/1024*
 *	Short	ZAccRaw		[1]			=										(data[17] << 8) | (data[18] << 0)	2b		zg = z * Grange/1024*
 *	Single	Latitude	[°"]		= (data[19] << 24) | (data[20] << 16) | (data[21] << 8) | (data[22] << 0)	4b							*
 *	Char	NSIndicator	[]			=														  (data[23] << 0)	1b		North / South		*
 *	Single	Longitude	[°"]		= (data[24] << 24) | (data[25] << 16) | (data[26] << 8) | (data[27] << 0)	4b							*
 *	Char	EWIndicator	[]			=														  (data[28] << 0)	1b		East  / West		*
 *	Single	MSLAltitude	[m]			= (data[29] << 24) | (data[30] << 16) | (data[31] << 8) | (data[32] << 0)	4b							*
 *  Byte	PacketEnd	[]			= 0x09																		1b							*
 ********************************************************************************************************************************************
*/

#include <SD.h>
#include "ADXL345.h"
#include <RFM69.h>			//Radio
#include <BMP180.h>			//Temp & Pres

#define Serial3Implemented

bool sending;
bool repeat;
String gpsIn;

const int GPSPin = 7;


const byte broadcastInitMessage = 0x04;
const byte mesurementStartMessage = 0x05;
const byte mesurementPauseMessage = 0x06;
const byte mesurementEndMessage = 0x07;
const byte packetStartMessage = 0x08;
const byte packetEndMessage = 0x09;
const byte takeAndSaveImageMessage = 0x0A;

const byte startMesCommand = 0x67;
const byte endMesCommand = 0x68;
const byte sendSampleCommand = 0x69;
const byte takeImageCommand = 0x6A;

BMP180 bmp;
ADXL345 adxl;

void setup()
{
	pinMode(GPSPin, OUTPUT);
	digitalWrite(GPSPin, HIGH);
	Serial.begin(115200);
	Serial1.begin(4800);
	Serial3.begin(9600);
	bmp.begin();
	adxl.begin();
	adxl.setRange(RNG_16G);
	//SD.begin();
	Serial.write(broadcastInitMessage);
}

void loop()
{
//Read Input
	while (Serial.available())
	{
		byte inByte = Serial.read();
		if (inByte == startMesCommand)
		{
			sending = true;
			repeat = true;
		}
		if (inByte == endMesCommand)
		{
			sending = false;
			repeat = false;
		}
		if (inByte == sendSampleCommand)
		{
			sending = true;
			repeat = false;
		}
		if (inByte == takeImageCommand)
		{
			takeImage();
		}
	}

	gpsIn = "";

	while (gpsIn.length() < 6 || gpsIn.substring(0, 6) != "$GPGGA")
	{
		byte inByte = Serial1.read();
		if ((char)inByte == '$')
		{
			gpsIn = (char)inByte + Serial1.readStringUntil('\r');
		}
	}
	

	//Write Output
	if (sending)
	{
		sending = repeat;
		Serial.write(mesurementStartMessage);
		double utcTime;
		double temp;
		double pres;
		short xacc;
		short yacc;
		short zacc;
		double lat;
		char ns;
		double lon;
		char ew;
		double alt;

		utcTime = atof(split(gpsIn, ',', 1).c_str());
		bmp.getData(temp, pres);
		adxl.readAcceleration(&xacc, &yacc, &zacc);
		lat = atof(split(gpsIn, ',', 2).c_str());
		ns = split(gpsIn, ',', 3)[0];
		lon = atof(split(gpsIn, ',', 4).c_str());
		ew = split(gpsIn, ',', 5)[0];
		alt = atof(split(gpsIn, ',', 9).c_str());

		String msg = String(utcTime) + "," + String(temp) + "," + String(pres) + "," + String(xacc) + "," + String(yacc) + "," + String(zacc) + "," + String(lat) + "," + String(ns) + "," + String(lon) + "," + String(ew) + "," + String(alt);

		Serial.write(packetStartMessage);
		Serial.print(msg);
		Serial.write(packetEndMessage);

		Serial.write(sending ? mesurementPauseMessage : mesurementEndMessage);

#if defined(Serial3Implemented)
		saveData(msg);
#else
		File dataFile = SD.open(backupFileName, FILE_WRITE);
		dataFile.println(msg);
		dataFile.close();
#endif
	}
}

String split(String data, char separator, int index)
{
	int found = 0;
	int strIndex[] = {
		0, -1 };
	int maxIndex = data.length() - 1;
	for (int i = 0; i <= maxIndex && found <= index; i++)
	{
		if (data.charAt(i) == separator || i == maxIndex)
		{
			found++;
			strIndex[0] = strIndex[1] + 1;
			strIndex[1] = (i == maxIndex) ? i + 1 : i;
		}
	}
	return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}

void takeImage()
{
	Serial3.write(takeAndSaveImageMessage);
}