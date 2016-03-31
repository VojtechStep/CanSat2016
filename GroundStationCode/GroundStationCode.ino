/*
 Name:		GroundStationCode.ino
 Created:	2/13/2016 9:44:10 PM
 Author:	Vojtěch
*/

// the setup function runs once when you press reset or power the board
#include <RFM69registers.h>
#include <RFM69.h>
#include <qbcan.h>
#include <BMP180.h>

#define NODEID 12
#define NETWORKID 9
#define ENCRYPTKEY "AlmightyLobsters"

RFM69 radio;
bool promiscuousMode = false;

void setup() {
	Serial.begin(9600);
	radio.initialize(FREQUENCY, NODEID, NETWORKID);
	radio.setHighPower();
	radio.encrypt(ENCRYPTKEY);
	radio.promiscuous(promiscuousMode);
}

// the loop function runs over and over again until power down or reset
void loop() {
	if (radio.receiveDone())
	{
		for (byte i = 0; i < radio.DATALEN; i++)
			Serial.write(radio.DATA[i]);
	}
}
