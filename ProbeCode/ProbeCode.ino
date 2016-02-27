/*
 Name:		ProbeCode.ino
 Created:	2/13/2016 9:43:30 PM
 Author:	Vojtěch && Jacob (Vojtěch is first, because he actually works)

 Packet Structure:
 **|Type|**|Name|******|Unit|*********|Data Decomposition|*****************************************************|Length|*|Comment|************
 *	Single	UTCTime		[hhmmss.sss]= (data[ 0] << 24) | (data[ 1] << 16) | (data[ 2] << 8) | (data[ 3] << 0)	4b							*
 *	Single	Temperature [°C]		= (data[ 4] << 24) | (data[ 5] << 16) | (data[ 6] << 8) | (data[ 7] << 0)	4b							*
 *	Single	Pressure	[mB]		= (data[ 8] << 24) | (data[ 9] << 16) | (data[10] << 8) | (data[11] << 0)	4b							*
 *	Short	XAccRaw		[1]			=										(data[12] << 8) | (data[13] << 0)	2b		xg = x * Grange/1024*
 *	Short	YAccRaw		[1]			=										(data[14] << 8) | (data[15] << 0)	2b		yg = y * Grange/1024*
 *	Short	ZAccRaw		[1]			=										(data[16] << 8) | (data[17] << 0)	2b		zg = z * Grange/1024*
 *	Single	Latitude	[°"]		= (data[18] << 24) | (data[19] << 16) | (data[20] << 8) | (data[21] << 0)	4b							*
 *	Char	NSIndicator	[]			=														  (data[22] << 0)	1b		North / South		*
 *	Single	Longitude	[°"]		= (data[23] << 24) | (data[24] << 16) | (data[25] << 8) | (data[26] << 0)	4b							*
 *	Char	EWIndicator	[]			=														  (data[27] << 0)	1b		East  / West		*
 *	Single	MSLAltitude	[m]			= (data[28] << 24) | (data[29] << 16) | (data[30] << 8) | (data[31] << 0)	4b							*
 ********************************************************************************************************************************************
*/

#include <RFM69.h>			//Radio
#include <BMP180.h>			//Temp & Pres

#define COMMANDLENGTH 32

byte data[COMMANDLENGTH];
bool sending;
bool repeat;

const byte startCommand[COMMANDLENGTH] { 0x05 };
const byte pauseCommand[COMMANDLENGTH] { 0x06 };
const byte endCommand[COMMANDLENGTH] { 0x07 };

union longdouble {
	long l;
	double d;
};

void setup() {
	Serial.begin(9600);
	Serial.println("BOOT");
}

void loop() {
	//Read Input
	while (Serial.available()) {
		byte inByte = Serial.read();
		if (inByte == 0x67) {
			sending = true;
			repeat = true;
		}
		if (inByte == 0x68) sending = false;
		if (inByte == 0x69) {
			sending = true;
			repeat = false;
		}
	}

	//Write Output
	if (sending) {
		sending = repeat;
		Serial.write(startCommand, COMMANDLENGTH);
		longdouble utcTime = { 161229.487 };
		longdouble temp = { 25.23 };
		longdouble pres = { 995.60 };
		short xacc = 128;
		short yacc = 128;
		short zacc = 128;
		longdouble lat = { 12158.3416 };
		unsigned char ns = 'N';
		longdouble lon = { 12158.3416 };
		unsigned char ew = 'W';
		longdouble alt = { 18.423 };

		data[0] = (utcTime.l & 0xFF000000) >> 24;
		data[1] = (utcTime.l & 0x00FF0000) >> 16;
		data[2] = (utcTime.l & 0x0000FF00) >> 8;
		data[3] = (utcTime.l & 0x000000FF);
		data[4] = (temp.l & 0xFF000000) >> 24;
		data[5] = (temp.l & 0x00FF0000) >> 16;
		data[6] = (temp.l & 0x0000FF00) >> 8;
		data[7] = (temp.l & 0x000000FF);
		data[8] = (pres.l & 0xFF000000) >> 24;
		data[9] = (pres.l & 0x00FF0000) >> 16;
		data[10] = (pres.l & 0x0000FF00) >> 8;
		data[11] = (pres.l & 0x000000FF);
		data[12] = (xacc & 0xFF00) >> 8;
		data[13] = (xacc & 0x00FF);
		data[14] = (yacc & 0xFF00) >> 8;
		data[15] = (yacc & 0x00FF);
		data[16] = (zacc & 0xFF00) >> 8;
		data[17] = (zacc & 0x00FF);
		data[18] = (lat.l & 0xFF000000) >> 24;
		data[19] = (lat.l & 0x00FF0000) >> 16;
		data[20] = (lat.l & 0x0000FF00) >> 8;
		data[21] = (lat.l & 0x000000FF);
		data[22] = ns;
		data[23] = (lon.l & 0xFF000000) >> 24;
		data[24] = (lon.l & 0x00FF0000) >> 16;
		data[25] = (lon.l & 0x0000FF00) >> 8;
		data[26] = (lon.l & 0x000000FF);
		data[27] = ew;
		data[28] = (lat.l & 0xFF000000) >> 24;
		data[29] = (lat.l & 0x00FF0000) >> 16;
		data[30] = (lat.l & 0x0000FF00) >> 8;
		data[31] = (lat.l & 0x000000FF);
		Serial.write(data, 32);
		Serial.write(pauseCommand, COMMANDLENGTH);
		delay(300);
	}
}



/*
const long utcTimeL = *reinterpret_cast<long*>(&utcTime);
Serial.println(utcTimeL, BIN);
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
data[ 0] = _byteFromBoolArray(utcTimeB, 0);
data[ 1] = _byteFromBoolArray(utcTimeB, 8);
data[ 2] = _byteFromBoolArray(utcTimeB, 16);
data[ 3] = _byteFromBoolArray(utcTimeB, 24);
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
byte temp[4];
int k = 32;
while (k != 0)
{
	temp[k / 8] = utcTimeL >> k;
	k -= 8;
}
for (int j = 0; j < 4; j++)Serial.print(temp[j], BIN);
Serial.println();
*/