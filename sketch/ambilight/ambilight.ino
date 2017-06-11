#include "FastLED.h"

//number of leds in your display. It's ok to use a number larger than you actually have.
#define NUM_LEDS 100 
//pin that controls the leds.
#define DATA_PIN 0

CRGB leds[NUM_LEDS];
byte buffer[64];
uint32_t timeToken;
uint16_t fps = 0;
uint16_t frames=0;
//int index;

void setup() {
  // put your setup code here, to run once:
  LEDS.addLeds<WS2812B,DATA_PIN,GRB>(leds,NUM_LEDS);
  //color correction
  FastLED.setCorrection(TypicalLEDStrip);
  LEDS.setBrightness(240);
  //clear all leds
  FastLED.clear();
  // set our time marker at current time
  timeToken=millis();
}

void loop() {
  // put your main code here, to run repeatedly:
  Read();
  Write();
  FPS();
  //increment frames because a frame has just occured.
  frames++;
}

//This function reads an incoming packet and copies the byte array into our led array. incoming packets are keyed with their location in the array.
void Read()
{
    int bytesRecieved = RawHID.recv(buffer,0);
    byte * pointer = &leds[0].r;
    pointer+=buffer[0]*63;
    if(bytesRecieved>0)
    {
      memcpy8(pointer,&buffer[1],64);
      FastLED.show();
    }
}

//Write a packet that contains our device frames per second data. FPS is a 16 bit integer so it is stored in the first two bytes of our packet.
void Write()
{
  byte outBuffer[64];
  //memset(outBuffer, 0, sizeof(outBuffer));
  //memcpy8(&outBuffer,&fps,2);
  outBuffer[0]=fps & 0xff;
  outBuffer[1]=(fps >> 8);
  RawHID.send(outBuffer,0);
}

//Calculates FPS. If more than a second has passed we set fps to the number of frames we have accumulated then reset frames and set our time token to the current time.
void FPS()
{
  uint32_t current=millis();
  uint16_t span = current-timeToken;
  if(span>=1000)
  {
    fps=frames;
    frames=0;
    timeToken=current;
  }
}

