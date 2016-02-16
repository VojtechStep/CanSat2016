/*
 Name:		ProbeBCode.ino
 Created:	2/14/2016 10:03:08 AM
 Author:	jacob
*/

#include <SPI\SPI.h>
#include <SD\src\SD.h>

void setup() {
	SPI.begin();
	Serial.begin(9600);
	cameraInit(0x2A);
}


void loop() {
	shootImg();
	readBuffer();
	saveBuffer();
}

void shootImg();
void cameraInit(byte);

void readBuffer()
{}

void saveBuffer()
{}