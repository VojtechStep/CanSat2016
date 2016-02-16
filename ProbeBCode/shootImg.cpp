#include <SoftwareSerial\SoftwareSerial.h>
#include <SD\src\SD.h>
#include <string>

//setting up pins and other crap for Camera
SoftwareSerial mySerial(5, 6);

byte ZERO = 0x00;
byte incomingByte;
long int i = 0x0000, j = 0, k = 0, count = 0;
uint8_t MH, ML;
bool end = false;
File myFile;



//functions for Camera
void SendResetCmd()
{
	mySerial.write(0x56);
	mySerial.write(ZERO);
	mySerial.write(0x26);
	mySerial.write(ZERO);
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
	mySerial.write(0x56);
	mySerial.write(ZERO);
	mySerial.write(0x54);
	mySerial.write(0x01);
	mySerial.write(Size);
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
void SetBaudRateCmd(byte baudrate)
{
	mySerial.write(0x56);
	mySerial.write(ZERO);
	mySerial.write(0x24);
	mySerial.write(0x03);
	mySerial.write(0x01);
	mySerial.write(baudrate);
}

void SendTakePhotoCmd()
{
	mySerial.write(0x56);
	mySerial.write(ZERO);
	mySerial.write(0x36);
	mySerial.write(0x01);
	mySerial.write(ZERO);
}

void SendReadDataCmd()
{
	MH = i / 0x100;
	ML = i % 0x100;
	mySerial.write(0x56);
	mySerial.write(ZERO);
	mySerial.write(0x32);
	mySerial.write(0x0c);
	mySerial.write(ZERO);
	mySerial.write(0x0a);
	mySerial.write(ZERO);
	mySerial.write(ZERO);
	mySerial.write(MH);
	mySerial.write(ML);
	mySerial.write(ZERO);
	mySerial.write(ZERO);
	mySerial.write(ZERO);
	mySerial.write(0x20);
	mySerial.write(ZERO);
	mySerial.write(0x0a);
	i += 0x20;
}

void StopTakePhotoCmd()
{
	mySerial.write(0x56);
	mySerial.write(ZERO);
	mySerial.write(0x36);
	mySerial.write(0x01);
	mySerial.write(0x03);
}



void cameraInit(byte baud)
{
	mySerial.begin(115200);
	delay(100);
	SendResetCmd();
	delay(2000);
	SetBaudRateCmd(baud);
	delay(500);
	mySerial.begin(38400);
	delay(100);
}

void shootImg(int iteration)
{
	byte b[32];

	SendResetCmd();
	delay(2000);
	SendTakePhotoCmd;
	delay(1000);
	while (mySerial.available() > 0)
	{
		incomingByte = mySerial.read();
	}

	string name = "img" + iteration + ".jpg";

	myFile = SD.open(name, FILE_WRITE);

	while (!end)
	{
		j = 0; k = 0; count = 0;
		SendReadDataCmd();
		delay(20);
		while (mySerial.available() > 0)
		{
			incomingByte = mySerial.read();
			k++;
			delay(1);
			if (k > 5 && j < 32 && !end)
			{
				b[j] = incomingByte;
				if ((b[j - 1] == 0xFF) && (b[j] == 0xD9))
				{
					end = true;
				}
				j++;
				count++;
			}
		}
		
		for (int l = 0; l < count; l++)
		{
			myFile.write(b[l]);
		}
	}

	myFile.close();
}