/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch && Jacob (Vojtěch is first, because he actually works)
*/

bool sending;

void setup() {
	Serial.begin(9600);
	Serial.println("BOOT");
}

void loop() {
	while (Serial.available()) {
		byte inByte = Serial.read();
		if (inByte == 0x67) sending = true;
		if (inByte == 0x68) sending = false;
	}
	if (sending) {
		Serial.println("START");
		for (int i = 0; i < 20; i++) {
			Serial.println("25.23,0.99560,1000,1000,1000,A,1000000,N,1000000,W");
			delay(300);
		}
		Serial.println("PAUSE");
		delay(2000);
	}
}