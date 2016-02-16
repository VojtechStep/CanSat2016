/*
 Name:		ProbeACode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch
*/

#include <SPI.h>
#include <EEPROM.h>
#include <string>
#include "readData.h"

// the setup function runs once when you press reset or power the board
void setup() {
	SPI.begin();
	string dataString;
}

// the loop function runs over and over again until power down or reset
void loop() {
	readData();
	saveData();
	sendData('A');
	if (BReady())
	{
		sendData('B');
	}
}

bool BReady() {};

void saveData(string & dataString)
{
	dataString = "";
}

void sendData(char receiver)
{
	switch (receiver)
	{
	case 'A':
		break;
	case 'B':
		break;
	}
}