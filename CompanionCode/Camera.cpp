/*MOSI - pin 11
MISO - pin 12
CLK  - pin 13
CS   - pin 10

TX - pin 5
RX - pin 6  */

#include <SoftwareSerial.h>
#include <SPI.h>
#include <SD.h>

SoftwareSerial camSerial(5, 6);

//definitions
byte ZERO = 0x00;
byte incomingbyte;
byte a[32];
long int j = 0, k = 0, count = 0, i = 0x0000;
uint8_t MH, ML;
boolean EndFlag = 0;
int iter = 0, l;
File camFile;

void setup() {
	Serial.begin(38400);
	while (!Serial)
		; //waiting for serial port to connect
	Serial.println("Serial connection established.");

	Serial.print("Initializing SD card... ");
	if (!SD.begin(10))
	{
		Serial.println("failed.");
		while (1);
	}
	Serial.println("done.");

	Serial.print("Setting up camera... ");
	camSerial.begin(115200);
	delay(100);
	SendResetCmd();
	delay(2000);
	SetBaudCmd(0x2A);
	delay(500);
	camSerial.begin(38400);
	delay(100);
	Serial.println("done.");
}

void loop() {
	Serial.print("Resetting camera");
	SendResetCmd();
	for (int z = 0; z < 4; z++)
	{
		Serial.print('.');
		delay(1000);
	}
	Serial.println(' ');
	Serial.print("Taking picture");
	SendTakePhotoCmd();
	for (iter; iter < 3; iter++)
	{
		Serial.write('.');
		delay(1000);
	}
	Serial.println("done.");

	Serial.print("Camera response: ");
	while (camSerial.available() > 0)
	{
		incomingbyte = camSerial.read();
		if (incomingbyte < 0x10)
			Serial.print('0');
		Serial.print(incomingbyte, HEX);
		Serial.println(' ');
	}
	Serial.println("done.");

	Serial.print("Opening file... ");
	if (SD.exists("img.jpg"))
	{
		Serial.print("deleting existing file... ");
		SD.remove("img.jpg");
	}
	camFile = SD.open("img.jpg", FILE_WRITE);
	Serial.println("done.");

	Serial.print("Saving data");
	for (iter; iter < 5; iter++)
	{
		Serial.write('.');
		delay(1000);
	}
	Serial.println(' ');
	while (!EndFlag)
	{
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
			if ((k>5) && (j<32) && (!EndFlag))
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

		for (j = 0; j < count; j++)  //behold the mighty (and utterly unobservable) HEX image
		{
			if (a[j] < 0x10)
				Serial.print("0");
			Serial.print(a[j], HEX);
			Serial.print(' ');
		}

		for (l = 0; l < count; l++)
			camFile.write(a[l]);
		Serial.println();
	}

	camFile.close();
	Serial.print("Saving completed.");
	while (1);
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