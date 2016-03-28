/*
 Name:		CompanionCode.ino
 Created:	3/9/2016 9:50:59 AM
 Author:	Adalbert
*/

#include <SoftwareSerial.h>
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
void efDelay();
void dataLogger();
void SendResetCmd();
void SetImageSizeCmd(byte Size);
void SetBaudCmd(byte baudrate);
void SendTakePhotoCmd();
void SendReadDataCmd();
void StopTakePhotoCmd();
/**********************/

SoftwareSerial camSerial(5, 6);

void setup()
{
	Serial.begin(115200);
	if (!SD.begin(10))
		return;
	camSerial.begin(115200);
	delay(100);
	SendResetCmd();
	delay(2000);
	SetBaudCmd(0x2A);
	delay(500);
	camSerial.begin(38400);
	delay(100);
}

// the loop function runs over and over again until power down or reset
void loop()
{
	if (iteration > 1) SendResetCmd();
	efDelay();
	//Camera set, start receiving data
	SendTakePhotoCmd();
	efDelay();
	while (camSerial.available() > 0)
	{
		incomingbyte = camSerial.read();
	}
	unsigned short nume = millis();
	String name = String(nume) + ".jpg";
	if (SD.exists(name))
		SD.remove(name);
	camFile = SD.open(name, FILE_WRITE);
	while (!EndFlag)
	{
		dataLogger();
		j = 0;
		k = 0;
		count = 0;
		SendReadDataCmd();
		delay(20);
		while (camSerial.available() > 0)
		{
			incomingbyte = camSerial.read();
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

}

void efDelay()
{
	last = millis() + 3000;
	cur = millis();
	while (millis() + cur < last)
	{
		dataLogger();
	}
}

void dataLogger()
{
	if (Serial.available() > 0)
	{
		inByte = Serial.read();
		if (inByte == packetStartMessage)
		{
			logFile = SD.open("data.log", FILE_WRITE);
			while (Serial.available() > 0)
			{
				inByte = Serial.read();
				if (inByte != packetEndMessage)
				{
					logFile.write(inByte);
				}
				else
				{
					logFile.close();
					break;
				}
			}
		}
	}
}

void SendResetCmd()
{
	camSerial.write(0x56);
	camSerial.write(ZERO);
	camSerial.write(0x26);
	camSerial.write(ZERO);
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
	camSerial.write(0x56);
	camSerial.write(ZERO);
	camSerial.write(0x54);
	camSerial.write(0x01);
	camSerial.write(Size);
}

/*************************************/
/* Set BaudRate :
/* <1>　0xAE  :   9600
/* <2>　0x2A  :   38400
/* <3>　0x1C  :   57600
/* <4>　0x0D  :   115200
/* <5>　0xAE  :   128000
/* <6>　0x56  :   256000
/*************************************/
void SetBaudCmd(byte baudrate)
{
	camSerial.write(0x56);
	camSerial.write(ZERO);
	camSerial.write(0x24);
	camSerial.write(0x03);
	camSerial.write(0x01);
	camSerial.write(baudrate);
}

void SendTakePhotoCmd()
{
	camSerial.write(0x56);
	camSerial.write(ZERO);
	camSerial.write(0x36);
	camSerial.write(0x01);
	camSerial.write(ZERO);
}

void SendReadDataCmd()
{
	MH = i / 0x100;
	ML = i % 0x100;
	camSerial.write(0x56);
	camSerial.write(ZERO);
	camSerial.write(0x32);
	camSerial.write(0x0c);
	camSerial.write(ZERO);
	camSerial.write(0x0a);
	camSerial.write(ZERO);
	camSerial.write(ZERO);
	camSerial.write(MH);
	camSerial.write(ML);
	camSerial.write(ZERO);
	camSerial.write(ZERO);
	camSerial.write(ZERO);
	camSerial.write(0x20);
	camSerial.write(ZERO);
	camSerial.write(0x0a);
	i += 0x20;
}

void StopTakePhotoCmd()
{
	camSerial.write(0x56);
	camSerial.write(ZERO);
	camSerial.write(0x36);
	camSerial.write(0x01);
	camSerial.write(0x03);
}