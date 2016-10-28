// Copyright (c) 2016 The Almighty Lobsters All Rights Reserved.
#include <Arduino.h>
#include <SPI.h>
#include <SD.h>

#include <BMP180.h>
#include "ADXL345.h"


const char handshakeExpect =   0x73;
const char handshakeResponse = 0x72;

void setup () {
  Serial.begin(9600);
  while (!Serial);
  while (Serial.available() < 1 || Serial.read() != handshakeExpect);
  Serial.print(handshakeResponse);
}

void loop () {
  Serial.println("Hello");
  delay(500);
}
