#include <Arduino.h>
#include <WiFi.h>
#include <FastLED.h>

#include "server.hpp"

constexpr const static char *WIFI_SSID = "BKW Display";
constexpr const static char *WIFI_PASSPHRASE = "GYI#2022";

BKServer server;
CRGB colors[30];

static void onStatus(const bool enabled)
{
  printf("status: %d\n", enabled);
}

static void onUpdate(const Color *buffer, const size_t length)
{
  static_assert(sizeof(CRGB) == sizeof(Color), "sizeof(CRGB) == sizeof(Color)");

  if (length < sizeof(colors) / sizeof(CRGB))
  {
    memcpy(colors, buffer, length * sizeof(Color));
  }

  printf("update: %d\n", length);
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

  printf("ready\n");
  printf("local addr: %s\n", WiFi.localIP().toString().c_str());
}

void loop()
{
}