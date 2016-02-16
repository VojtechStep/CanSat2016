/*
 Name:		ProbeBCode.ino
 Created:	2/14/2016 10:03:08 AM
 Author:	jacob
*/

#include <SPI\SPI.h>
#include <SD\src\SD.h>
#include <string>

int iteration = 0;
string data;
File dataFile;

void setup() {
	SPI.begin();
	cameraInit(0x2A);
	dataFile = SD.open("Mesures", FILE_WRITE);
}


void loop() {
	shootImg(iteration);
	readBuffer();
	saveBuffer();
}

void shootImg(int);
void cameraInit(byte);

void readBuffer()
{}

void saveBuffer()
{
	dataFile = SD.open("Mesures");
	if (dataFile)
	{
		dataFile.println(data);
		dataFile.close;
	}
}