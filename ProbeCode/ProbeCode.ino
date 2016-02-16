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

long initStartMillis;
long startMillis;
long totalTimeout = 60000;
long timeout = 10000;
bool listening = true;

char data[50];

void setup()
{
	Serial.begin(9600);
	initStartMillis = millis();
	bmp.begin();
}

void loop()
{
	if(listening) {
		if (millis() < initStartMillis + totalTimeout) {
			Serial.println("START");
			startMillis = millis();
			while (millis() < startMillis + timeout)
			{
				bmp.getData(T, P);
				String temp = String(T, 2);
				String pres = String(P/1000, 5);

				sprintf(data, "%s,%s,1000,1000,1000,A,1000000,N,1000000,W", temp.c_str(), pres.c_str());
				Serial.println(data);
				delay(100);
			}
			Serial.println("PAUSE");
			delay(5000);
		}
		else {
			Serial.println("END");
			listening = false;
		}
	}
}
