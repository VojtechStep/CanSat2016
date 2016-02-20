/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch
*/

#include <qbcan.h>

BMP180 bmp;
double T, P;

long startMillis;
long timeout = 5000;

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
		String temp = String(T, 2);
		String pres = String(P / 1000, 5);

		sprintf(data, "%s,%s,1000,1000,1000,A,1000000,N,1000000,W", temp.c_str(), pres.c_str());
		Serial.println(data);
		delay(500);
	}
	Serial.println("PAUSE");
	delay(2000);
}
