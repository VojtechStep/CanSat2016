// Copyright (c) 2016 The Almighty Lobsters All Rights Reserved.
#include <SerialCommand.h>
#include "main.h"
#include <SPI.h>
#include <SD.h>

#include <BMP180.h>
#include "ADXL345.h"
#include "packet.h"

SerialCommand sCmd;

const byte test = 0x74;

void setup ()
{
  Serial.begin(9600);
  sCmd.addCommand("get", printData);
  sCmd.setDefaultHandler(unknown);

  while(!Serial);
  while(true)
  {
    if(Serial.available() > 0)
    {
      int incoming = Serial.read();
      if(incoming == handshakeExpect) break;
    }
  }
  Serial.write(handshakeResponse);
}

void loop ()
{
  sCmd.readSerial();
}

void printData()
{
  Packet<128> dataPacket;
  dataPacket.append(test);
  unsigned short pckLength = dataPacket.getPacketSize();
  byte* pckData = reinterpret_cast<byte *>(malloc(pckLength));
  dataPacket.pack(pckData);

  Serial.write(pckData, pckLength);
}

void unknown(const char* command)
{
  Serial.write(errorResponse);
}
