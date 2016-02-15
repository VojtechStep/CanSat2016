/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch
*/

// the setup function runs once when you press reset or power the board
#include <RFM69registers.h>
#include <RFM69.h>
#include <qbcan.h>
#include <BMP180.h>

BMP180 bmp;
double T, P;

long startMillis;
long timeout = 10000;

char data[50];

void setup()
{
	Serial.begin(9600);
	bmp.begin();
}

void loop()
{
	Serial.println("START");
	startMillis = millis();
	while (millis() < startMillis + timeout)
	{
		bmp.getData(T, P);

		sprintf(data, "%d,%d,1000,1000,1000,A,1000000,N,1000000,W", T * 1000, P * 10);
		Serial.println(data);
		delay(100);
	}
	Serial.println("PAUSE");
	delay(5000);
}
