#include <SerialCommand.h>

//#define led 13

SerialCommand sCmd;

void ledOn();
void ledOff();
void printHello();
void unrecognized(const char* command);

void setup() {

  Serial.begin(9600);

  sCmd.addCommand("ON", ledOn);
  sCmd.addCommand("OFF", ledOff);
  sCmd.addCommand("HELLO", printHello);
  sCmd.setDefaultHandler(unrecognized);
  Serial.println("Ready");
}

void loop() {
  sCmd.readSerial();
}

void ledOn() {
  Serial.println("LED on");
  //digitalWrite(led, HIGH);
}

void ledOff() {
  Serial.println("LED off");
  //digitalWrite(led, LOW);
}

void printHello() {
  char* args;
  args = sCmd.next();
  if(args != nullptr)
  {
    Serial.print("Hello ");
    Serial.println(args);
  }
  else
  {
    Serial.println("Hello hooman");
  }
}

void unrecognized(const char* command) {
  Serial.println("Sorry, I didn't get that. Try again in a little bit.");
}
