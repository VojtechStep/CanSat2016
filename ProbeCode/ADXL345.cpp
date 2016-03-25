#include "ADXL345.h"
#include "Arduino.h"
#include <Wire.h>

void ADXL345::begin()
{
	Wire.begin();
	writeTo(0x53, 0x2D, 0);
	writeTo(0x53, 0x2D, 16);
	writeTo(0x53, 0x2D, 8);
}

void ADXL345::readAcceleration(short* x, short* y, short* z)
{
	byte buffer[6];
	readFrom(0x53, 0x32, 6, buffer);

	(*x) = (((int)buffer[1]) << 8) | buffer[0];
	(*y) = (((int)buffer[3]) << 8) | buffer[2];
	(*z) = (((int)buffer[5]) << 8) | buffer[4];
}

void ADXL345::setRange(const byte& rng)
{
	writeTo(0x53, 0x31, rng);
}

void ADXL345::writeTo(int deviceAddress, byte regAddress, byte value)
{
	Wire.beginTransmission(deviceAddress);
	Wire.write(regAddress);
	Wire.write(value);
	Wire.endTransmission();
}

void ADXL345::readFrom(int deviceAddress, byte startAddress, int count, byte* buffer)
{
	Wire.beginTransmission(deviceAddress);
	Wire.write(startAddress);
	Wire.endTransmission();

	Wire.beginTransmission(deviceAddress);
	Wire.requestFrom(deviceAddress, count);

	int i = 0;
	while (Wire.available())
	{
		buffer[i] = Wire.read();
		i++;
	}
	Wire.endTransmission();
}
