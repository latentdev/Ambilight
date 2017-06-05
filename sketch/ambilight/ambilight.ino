#include "FastLED.h"

#define NUM_LEDS 300 
#define DATA_PIN 5
#define VERT 52
#define HORIZON 90

CRGB leds[NUM_LEDS];
byte buffer[64];
//int index;

void setup() {
  // put your setup code here, to run once:
  LEDS.addLeds<WS2812B,DATA_PIN,GRB>(leds,NUM_LEDS);
  FastLED.setCorrection(TypicalLEDStrip);
  LEDS.setBrightness(150);
  FastLED.clear();
}

void loop() {
  // put your main code here, to run repeatedly:
  Read();
  FastLED.show();
}
/*void Read()
{
  index=0;
  for (int i=0;i<14;i++)
  {
    ReadPacket();
  }
}*/
void Read()
{
  for(int i=0;i<10;i++)
  {
    int bytesRecieved = RawHID.recv(buffer,0);
    byte * pointer = &leds[0].r;
    pointer+=buffer[0]*63;
    if(bytesRecieved>0)
    {
      memcpy8(pointer,&buffer[1],63);
    }
  }
}

//void Write()
//{
  //byte outBuffer[64];
  //RawHID.send(outBuffer,0);
//}

/*void Draw()
{
  for(int i=0;i<NUM_LEDS;i++)
  {
      leds[i].g = 255;
      leds[i].r = 100;
      leds[i].b = 100;
  }
}*/

