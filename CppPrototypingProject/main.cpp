#include <iostream>
#include <string>

using namespace std;


string* split(const string&, const char&, int*);
int indexOf(const string&, const char&);
int indexOf(const string&, const char&, const int&);

int main(int argc, char** args)
{
	string phrase = "Hello,world,how,are,you";
	int wordsCount;
	string* words = split(phrase, ',', &wordsCount);

	for (int i = 0; i < wordsCount; i++)
		cout << words[i] << endl;

	cout << "Hello world" << endl;
	system("pause");
	return 0;
}

string* split(const string& str, const char& ch, int* length)
{
	*length = 0;
	int lastFoundIndex = 0;
	while (lastFoundIndex != -1)
	{
		(*length)++;
		lastFoundIndex = indexOf(str, ch, lastFoundIndex + 1);
	}
	if (*length == 0) return false;
	string* strings = new string[*length];
	int start = 0;
	int end = indexOf(str, ch);
	for (int i = 0; i < *length; i++)
	{
		strings[i] = str.substr(start, end - start);
		start = end;
		end = indexOf(str, ch, start + 1);
		if (end == -1) end = str.length() - 1;
	}
	return strings;
}

int indexOf(const string& str, const char& ch)
{
	int i = -1;
	for (i++; i < str.length(); i++)
	{
		if (str.at(i) != ch) continue;
	}
	return -1;
}

int indexOf(const string& str, const char& ch, const int& startIndex)
{
	return indexOf(str.substr(startIndex, str.length() - startIndex), ch) + startIndex;
}