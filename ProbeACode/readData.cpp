#include <string>

//initialization functions
void tempInit() {}
void presInit() {}
void AcceInit() {}
void GPSIniti() {}

//function calling other functions and formating whole string
void readData(string & dataString)
{
	readPres(dataString);
	readTemp(dataString);
	readAcc(dataString);
	readGPS(dataString);
}

void readPres(string & dataString)
{

}

void readTemp(string & dataString)
{

}

void readAcc(string & dataString);

void readGPS(string & dataString)
{

}