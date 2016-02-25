/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch
*/

#include <qbcan.h>

BMP180 bmp;
double T, P;

unsigned long startMillis;
unsigned long timeout = 5000;
bool sending;

byte inByte;
String command = "";

char data[50];

void setup()
{
	Serial.begin(115200);
	bmp.begin();
	while (!Serial);
	Serial.println("REBOOTING");
	sending = false;
}

void loop()
{
	command = "";
	while (Serial.available())
	{
		inByte = Serial.read();
		command += (char)inByte;
	}
	if ((byte)command.charAt(0) == 0x67) sending = true;
	if ((byte)command.charAt(0) == 0x68) sending = false;
	if (sending)
	{
		Serial.println("START");
		long now = millis();
		while (millis() < now + 5000)
		{
			double T, P;
			bmp.getData(T, P);
			String temp = String(T, 2);
			String pres = String(P / 1000, 5);
			sprintf(data, "%s,%s,1000,1000,1000,A,1000000,N,1000000,W", temp.c_str(), pres.c_str());
			Serial.println(data);
			delay(200);
		}
		Serial.println("PAUSE");
		delay(2000);
	}
}
