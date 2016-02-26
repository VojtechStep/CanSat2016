/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch && Jacob (Vojtěch is first, because he actually works)

 Packet Structure:
 **|Type|**|Name|******|Unit|*********|Data Decomposition|*****************************************************|Length|*|Comment|************
 *	Int64	UTCTime		[hhmmss.sss]= (data[ 0] << 24) | (data[ 1] << 16) | (data[ 2] << 8) | (data[ 3] << 0)	4b							*
 *	Int64	Temperature [°C]		= (data[ 4] << 24) | (data[ 5] << 16) | (data[ 6] << 8) | (data[ 7] << 0)	4b							*
 *	Int64	Pressure	[mB]		= (data[ 8] << 24) | (data[ 9] << 16) | (data[10] << 8) | (data[11] << 0)	4b							*
 *	Short	XAccRaw		[1]			=										(data[12] << 8) | (data[13] << 0)	2b		xg = x * Grange/1024*
 *	Short	YAccRaw		[1]			=										(data[14] << 8) | (data[15] << 0)	2b		yg = y * Grange/1024*
 *	Short	ZAccRaw		[1]			=										(data[16] << 8) | (data[17] << 0)	2b		zg = z * Grange/1024*
 *	Int64	Latitude	[°"]		= (data[18] << 24) | (data[19] << 16) | (data[20] << 8) | (data[21] << 0)	4b							*
 *	Char	NSIndicator	[]			=														  (data[22] << 0)	1b		North / South		*
 *	Int64	Longitude	[°"]		= (data[23] << 24) | (data[24] << 16) | (data[25] << 8) | (data[26] << 0)	4b							*
 *	Char	EWIndicator	[]			=														  (data[27] << 0)	1b		East  / West		*
 *	Int64	MSLAltitude	[m]			= (data[28] << 24) | (data[29] << 16) | (data[30] << 8) | (data[31] << 0)	4b							*
 ********************************************************************************************************************************************
*/

#include <RFM69.h>			//Radio
#include <BMP180.h>			//Temp & Pres


byte data[32];
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
			double utcTime = 161229.487;
			const bool* utcTimeB = reinterpret_cast<bool*>(&utcTime);
			double temp = 25.23;
			const bool* tempB = reinterpret_cast<bool*>(&temp);
			double pres = 995.60;
			const bool* presB = reinterpret_cast<bool*>(&pres);
			short  xacc = 128;
			short  yacc = 128;
			short  zacc = 128;
			double lat = 3723.2475;
			const bool* latB = reinterpret_cast<bool*>(&lat);
			char   ns = 'N';
			double lon = 12158.3416;
			const bool* lonB = reinterpret_cast<bool*>(&lon);
			char   ew = 'W';
			double alt = 18.423;
			const bool* altB = reinterpret_cast<bool*>(&alt);
			data[ 0] = _byteFromBoolArray(utcTimeB, 24);
			data[ 1] = _byteFromBoolArray(utcTimeB, 16);
			data[ 2] = _byteFromBoolArray(utcTimeB, 8);
			data[ 3] = _byteFromBoolArray(utcTimeB, 0);
			data[ 4] = _byteFromBoolArray(tempB, 24);
			data[ 5] = _byteFromBoolArray(tempB, 16);
			data[ 6] = _byteFromBoolArray(tempB, 8);
			data[ 7] = _byteFromBoolArray(tempB, 0);
			data[ 8] = _byteFromBoolArray(presB, 24);
			data[ 9] = _byteFromBoolArray(presB, 16);
			data[10] = _byteFromBoolArray(presB, 8);
			data[11] = _byteFromBoolArray(presB, 0);
			data[12] = ((byte)xacc >> 8);
			data[13] = ((byte)xacc >> 0);
			data[14] = ((byte)yacc >> 8);
			data[15] = ((byte)yacc >> 0);
			data[16] = ((byte)zacc >> 8);
			data[17] = ((byte)zacc >> 0);
			data[18] = _byteFromBoolArray(latB, 24);
			data[19] = _byteFromBoolArray(latB, 16);
			data[20] = _byteFromBoolArray(latB, 8);
			data[21] = _byteFromBoolArray(latB, 0);
			data[22] = (byte)ns;
			data[23] = _byteFromBoolArray(lonB, 24);
			data[24] = _byteFromBoolArray(lonB, 16);
			data[25] = _byteFromBoolArray(lonB, 8);
			data[26] = _byteFromBoolArray(lonB, 0);
			data[27] = (byte)ew;
			data[28] = _byteFromBoolArray(altB, 24);
			data[29] = _byteFromBoolArray(altB, 16);
			data[30] = _byteFromBoolArray(altB, 8);
			data[31] = _byteFromBoolArray(altB, 0);

			//Serial.println("25.23,0.99560,1000,1000,1000,A,1000000,N,1000000,W");
			sendPacket(data, 32);
			delay(300);
		}
		Serial.println("PAUSE");
		delay(2000);
	}
}

byte _byteFromBool(const bool& i1, const bool& i2, const bool& i3, const bool& i4, const bool& i5, const bool& i6, const bool& i7, const bool& i8)
{
	return ((byte)i1 << 7) | ((byte)i2 << 6) | ((byte)i3 << 5) | ((byte)i4 << 4) | ((byte)i5 << 3) | ((byte)i6 << 2) | ((byte)i7 << 1) | ((byte)i8 << 0);
}

byte _byteFromBoolArray(const bool*& ar, int offset)
{
	return _byteFromBool(
		ar[offset + 0],
		ar[offset + 1],
		ar[offset + 2],
		ar[offset + 3],
		ar[offset + 4],
		ar[offset + 5],
		ar[offset + 6],
		ar[offset + 7]
		);
}

void sendPacket(const byte buffer[], const int& count)
{
	for (int i = 0; i < count; i++) Serial.write(buffer[i]);
	Serial.print('\n');
}
