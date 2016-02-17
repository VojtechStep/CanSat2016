#include <SPI.h>

#define CS 10	//chip Select pin
#define GR 8	//g-range

#define DATA_FORMAT	0x31
#define DATAX0		0x32
#define DATAX1		0x33
#define DATAY0		0x34
#define DATAY1		0x35
#define DATAZ0		0x36
#define DATAZ1		0x37

//buffers to hold ADXL values
char values[10];

//variables
int x, y, z;
double xg, yg, zg;
char charType = 0;

void readAcc(string & dataString)
{
	readRegister(DATAX0, 6, values);

	x = ((int)values[1] << 8) | (int)values[0];
	y = ((int)values[3] << 8) | (int)values[2];
	z = ((int)values[5] << 8) | (int)values[4];

	//Gs = Measurement Value * (G-range/(2^10))
	dataString += to_string(x * (GR / 1024));
	dataString += to_string(y * (GR / 1024));
	dataString += to_string(z * (GR / 1024));
}