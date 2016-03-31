/*
 Name:		CompanionCode.ino
 Created:	3/9/2016 9:50:59 AM
*/

#include <SD.h>
#include "ADXL345.h"
#include <RFM69registers.h>
#include <RFM69.h>
#include <qbcan.h>
#include <BMP180.h>

/*
	Serial - Debug/Probe -- 115200
	Serial1 - GPS
	Serial2 - Camera
*/


#define NODEID 10
#define NETWORKID 169
#define GATEWAYID 8
#define ENCRYPTKEY "AlmightyLobsters"

bool sending = true, repeat = true;
byte inByte;
String gpsIn;
String msgBuffer;

long curMillis;

const byte broadcastInitMessage = 0x04;
const byte packetStartMessage = 0x08;
const byte packetEndMessage = 0x09;

const byte startMesCommand = 0x67;
const byte endMesCommand = 0x68;
const byte sendSampleCommand = 0x69;

BMP180 bmp;
ADXL345 adxl;
RFM69 radio;

File logFile;

/*camera definitions*/
byte ZERO = 0x00;
byte incomingbyte;
byte a[32];
long int j = 0, k = 0, count = 0, i = 0x0000;
uint8_t MH, ML;
boolean EndFlag = 0;
int iter = 0, l;
File camFile;
/********************/

unsigned long cur = 0, last = 0;
int delta = 0;

long iteration = 0;

/******functions******/
void SendResetCmd();
void SetImageSizeCmd(byte Size);
void SetBaudCmd(byte baudrate);
void SendTakePhotoCmd();
void SendReadDataCmd();
void StopTakePhotoCmd();
/**********************/


void setup()
{
	if (!SD.begin(4))
	{
		while (1);
	}
	Serial1.begin(4800);
	Serial2.begin(115200);
	bmp.begin();
	adxl.begin();
	adxl.setRange(3);
  radio.initialize(FREQUENCY, NODEID, NETWORKID);
  radio.setHighPower();
  radio.encrypt(ENCRYPTKEY);
	delay(100);
	SendResetCmd();
	curMillis = millis();
  while(millis() < curMillis + 2000)
  {
    mainLoopThrough();
  }
	SetBaudCmd(0x2A);
	delay(500);
	Serial2.begin(38400);
	radio.send(GATEWAYID, &broadcastInitMessage, 1);
}

// the loop function runs over and over again until power down or reset
void loop()
{
	EndFlag = 0;
	SendResetCmd();
  curMillis = millis();
  while(millis() < curMillis + 3000)
  {
    mainLoopThrough();
  }
	SendTakePhotoCmd();
  curMillis = millis();
  while(millis() < curMillis + 3000)
  {
    mainLoopThrough();
  }
	while (Serial2.available() > 0)
	{
		incomingbyte = Serial2.read();
	}
	String name = "pic" + String(iteration) + ".jpg";
	if (SD.exists(name))	SD.remove(name);
	camFile = SD.open(name, FILE_WRITE);
	msgBuffer = "";
	while (!EndFlag)
	{
		mainLoopThrough();
		j = 0;
		k = 0;
		count = 0;
		SendReadDataCmd();
		delay(20);
		while (Serial2.available() > 0)
		{
			incomingbyte = Serial2.read();
			k++;
			delay(1); //250 for regular
			if ((k > 5) && (j < 32) && (!EndFlag))
			{
				a[j] = incomingbyte;
				if ((a[j - 1] == 0xFF) && (a[j] == 0xD9))     //tell if the picture is finished
				{
					EndFlag = 1;
				}
				j++;
				count++;
			}
		}
		for (l = 0; l < count; l++)
			camFile.write(a[l]);
	}
	iteration++;
}


void mainLoopThrough() {
	//Computer In
	if(radio.receiveDone())
	{
		inByte = radio.DATA[0];
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
	}

	// GPS
	if (Serial1.available() > 0)
	{
		inByte = Serial1.read();
		if ((char)inByte == '$')
		{
			gpsIn = Serial1.readStringUntil('\r');
			if (split(gpsIn, ',', 0) == "GPGGA") {
				if (sending) {
					sending = repeat;
					double utcTime = atof(split(gpsIn, ',', 1).c_str());
					double temp, pres;
					bmp.getData(temp, pres);
					short xacc, yacc, zacc;
					adxl.readAcceleration(&xacc, &yacc, &zacc);
					xacc *= 16 / 1024;
					yacc *= 16 / 1024;
					zacc *= 16 / 1024;
					double lat = atof(split(gpsIn, ',', 2).c_str());
					char ns = split(gpsIn, ',', 3)[0];
					double lon = atof(split(gpsIn, ',', 4).c_str());
					char ew = split(gpsIn, ',', 5)[0];
					double alt = atof(split(gpsIn, ',', 9).c_str());

					String msg = String(utcTime) + "," + String(temp) + "," + String(pres) + "," + String(xacc) + "," + String(yacc) + "," + String(zacc) + "," + String(lat) + "," + String(ns) + "," + String(lon) + "," + String(ew) + "," + String(alt);

          radio.send(GATEWAYID, ((char)packetStartMessage + msg + (char)packetEndMessage).c_str(), msg.length() + 2);
					logFile = SD.open("data.log", FILE_WRITE);
					logFile.print(msg);
					logFile.close();
				}
			}
		}
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


void SendResetCmd()
{
	Serial2.write(0x56);
	Serial2.write(ZERO);
	Serial2.write(0x26);
	Serial2.write(ZERO);
}

/*************************************/
/* Set ImageSize :
/* <1> 0x22 : 160*120
/* <2> 0x11 : 320*240
/* <3> 0x00 : 640*480
/* <4> 0x1D : 800*600
/* <5> 0x1C : 1024*768
/* <6> 0x1B : 1280*960
/* <7> 0x21 : 1600*1200
/************************************/
void SetImageSizeCmd(byte Size)
{
	Serial2.write(0x56);
	Serial2.write(ZERO);
	Serial2.write(0x54);
	Serial2.write(0x01);
	Serial2.write(Size);
}

/*************************************/
/* Set BaudRate :
/* <1> 0xAE  :   9600
/* <2> 0x2A  :   38400
/* <3> 0x1C  :   57600
/* <4> 0x0D  :   115200
/* <5> 0xAE  :   128000
/* <6> 0x56  :   256000
/*************************************/
void SetBaudCmd(byte baudrate)
{
	Serial2.write(0x56);
	Serial2.write(ZERO);
	Serial2.write(0x24);
	Serial2.write(0x03);
	Serial2.write(0x01);
	Serial2.write(baudrate);
}

void SendTakePhotoCmd()
{
	Serial2.write(0x56);
	Serial2.write(ZERO);
	Serial2.write(0x36);
	Serial2.write(0x01);
	Serial2.write(ZERO);
}

void SendReadDataCmd()
{
	MH = i / 0x100;
	ML = i % 0x100;
	Serial2.write(0x56);
	Serial2.write(ZERO);
	Serial2.write(0x32);
	Serial2.write(0x0c);
	Serial2.write(ZERO);
	Serial2.write(0x0a);
	Serial2.write(ZERO);
	Serial2.write(ZERO);
	Serial2.write(MH);
	Serial2.write(ML);
	Serial2.write(ZERO);
	Serial2.write(ZERO);
	Serial2.write(ZERO);
	Serial2.write(0x20);
	Serial2.write(ZERO);
	Serial2.write(0x0a);
	i += 0x20;
}

void StopTakePhotoCmd()
{
	Serial2.write(0x56);
	Serial2.write(ZERO);
	Serial2.write(0x36);
	Serial2.write(0x01);
	Serial2.write(0x03);
}
