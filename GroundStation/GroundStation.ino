
//Include the required libraries
#include <qbcan.h>
#include <Wire.h>
#include <SPI.h>


//Radio Parameters
#define NODEID        8    //unique for each node on same network
#define NETWORKID     169  //the same on all nodes that talk to each other
#define ENCRYPTKEY    "AlmightyLobsters" //exactly the same 16 characters/bytes on all nodes!
#define PROBEID       10

byte inByte;
String inMsg = "";

//Radio object
RFM69 radio;
bool promiscuousMode = false; //set to 'true' to sniff all packets on the same network

void setup()
{
  Serial.begin(115200);
  delay(1000);
  radio.initialize(FREQUENCY,NODEID,NETWORKID);
  radio.setHighPower(); //Use the high power capabilities of the RFM69HW
  radio.encrypt(ENCRYPTKEY);
  radio.promiscuous(promiscuousMode);
  Serial.println("Setup complete");
}


byte ackCount=0;
uint32_t packetCount = 0;
void loop()
{
  if(Serial.available())
  {
    inMsg = "";
    while(Serial.available())
    {
      inMsg += (char)Serial.read();
    }
    radio.send(PROBEID, inMsg.c_str(), inMsg.length());
    Serial.println("Resent " + inMsg);
  }
  if (radio.receiveDone())
  {
    Serial.println("Receiving data");
    for(byte i = 0; i < radio.DATALEN; i++)
      Serial.write(radio.DATA[i]);
  }
}



