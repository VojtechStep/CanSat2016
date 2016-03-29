#include "registerOperations.h"

void writeRegister(int cs, byte thisRegister, byte thisValue) {
	thisRegister = thisRegister << 2;
	byte dataToSend = thisRegister | WRITE;

	digitalWrite(cs, LOW);

	SPI.transfer(dataToSend);
	SPI.transfer(thisValue);

	digitalWrite(cs, HIGH);
}

unsigned int readRegister(int cs, byte thisRegister, int bytesToRead) {
	byte inByte = 0;           // incoming byte from the SPI
	unsigned int result = 0;   // result to return
	Serial.print(thisRegister, BIN);
	Serial.print("\t");
	thisRegister = thisRegister << 2;
	byte dataToSend = thisRegister & READ;
	Serial.println(thisRegister, BIN);
	digitalWrite(cs, LOW);
	SPI.transfer(dataToSend);
	result = SPI.transfer(0x00);
	bytesToRead--;
	if (bytesToRead > 0) {
		result = result << 8;
		inByte = SPI.transfer(0x00);
		result = result | inByte;
		bytesToRead--;
	}
	digitalWrite(cs, HIGH);
	return (result);
}