// ADXL345.h

#ifndef _ADXL345_h
#define _ADXL345_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "Wire.h"

#define RNG_2G 0
#define RNG_4G 1
#define RNG_8G 2
#define RNG_16G 3

class ADXL345
{
protected:

public:
	void begin();
	void readAcceleration(short*, short*, short*);
	void setRange(const byte&);
private:
	void writeTo(int, byte, byte);
	void readFrom(int, byte, int, byte*);
};

#endif

