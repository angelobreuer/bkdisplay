#include <Arduino.h>
#include <WiFi.h>
#include <FastLED.h>

#include "server.hpp"

constexpr const static int ROWS=11;
constexpr const static int COLS=13;

constexpr const static char *WIFI_SSID = "BKW Display";
constexpr const static char *WIFI_PASSPHRASE = "GYI#2022";

BKServer server;
CRGB colors[ROWS*COLS];

static void onStatus(const bool enabled)
{
  printf("status: %d\n", enabled);

  if (enabled)
  {
    FastLED.show();
  }
  else
  {
    FastLED.clear();
  }
}

static void onUpdate(const Color *buffer, const size_t length)
{
  static_assert(sizeof(CRGB) == sizeof(Color), "sizeof(CRGB) == sizeof(Color)");

  if (length != sizeof(colors) / sizeof(CRGB))
  {
    printf("update len err\n");
    return;
  }

  for (int row = 0; row <ROWS;row++){
    const Color *source = buffer + row * COLS;
    CRGB *destination = colors + row * COLS;

    if (row % 2){
      memcpy(destination, source, COLS);
    }else{
      for (int index = 0; index < COLS; index++){
        destination[COLS - index - 1] = reinterpret_cast<const CRGB *>(source)[index];
      }
    }
  }

  printf("update: %d\n", length);
  FastLED.show();
}

void setup()
{
  // WiFi.softAP(WIFI_SSID, WIFI_PASSPHRASE, 8, false, 4);
  WiFi.setHostname(WIFI_SSID);

  WiFi.begin("BKW-GYI", "GYI2022#!2023");
  WiFi.waitForConnectResult();

  server.begin();
  server.onStatus(onStatus);
  server.onUpdate(onUpdate);

  FastLED.addLeds<WS2812B, 18, GRB>(colors, 143);

  printf("ready\n");
  printf("local addr: %s\n", WiFi.localIP().toString().c_str());
}

void loop()
{
}