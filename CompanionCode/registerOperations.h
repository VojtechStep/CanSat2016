#pragma once
#include <SPI.h>

const byte READ = 0b11111100;
const byte WRITE = 0b00000010;

void writeRegister(int cs, byte reg, byte value);
unsigned int readRegister(int cs, byte reg, int bytesToRead);