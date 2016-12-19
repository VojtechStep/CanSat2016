#include <Arduino.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>

class BinHelper
{
public:
  template <typename T>
  static unsigned short bincpy(void *dest, const T src)
  {
    memcpy(dest, src, sizeof(*src));
    return sizeof(*src);
  }
};
template <>
unsigned short BinHelper::bincpy(void *dest, const char *src)
{
  memcpy(dest, &src, strlen(src));
  return strlen(src);
}
template <>
unsigned short BinHelper::bincpy(void *dest, const String src)
{
  memcpy(dest, src.c_str(), src.length());
  return src.length();
}

template <size_t maxLength>
class Packet
{
public:
  static const unsigned short REVNUM = (0x01);

  Packet(void)
  {
    this->data = (byte *)malloc(maxLength);
    this->length = 0;
  }
  ~Packet(void) { free(this->data); }
  template <typename T>
  void append(const T d)
  {
    this->length += BinHelper::bincpy(this->data + this->length, &d);
  }
  void pack(byte *p_pck)
  {
    header(p_pck);
    memcpy(p_pck + 2 * sizeof(unsigned short), data, this->length);
    checksum(p_pck + 2 * sizeof(unsigned short) + this->length);
  }

  void pHeader(byte *hdr) { header(hdr); }

  unsigned short getPacketSize()
  {
    return this->length + 2 * sizeof(unsigned short) + 1;
  }

private:
  unsigned short int length;
  byte *data;

  void header(byte *hdr)
  {
    memcpy(hdr, &(Packet<maxLength>::REVNUM), sizeof(unsigned short));
    memcpy(hdr + sizeof(unsigned short), &this->length, sizeof(unsigned short));
  }
  void checksum(byte *cksum)
  {
    *cksum = 0;
    for (unsigned int i = 0; i < this->length; i++)
      *cksum ^= this->data[i];
  }
};
