#pragma once

#include <Stream.h>
#include <AsyncUDP.h>

struct Color
{
    uint8_t r;
    uint8_t g;
    uint8_t b;
};

typedef std::function<void(const Color *colors, const size_t count)> UpdateHandlerFunction;
typedef std::function<void(const bool enabled)> StatusHandlerFunction;

class BKServer
{
public:
    constexpr const static int MAXIMUM_COLORS = 128;

    void begin();

    void onUpdate(const UpdateHandlerFunction handler);
    void onStatus(const StatusHandlerFunction handler);

private:
    static void _handlePacketStatic(void *arg, AsyncUDPPacket &packet);
    void _handlePacket(AsyncUDPPacket &packet);

    void _processUpdate(const uint8_t *buffer, const size_t length);
    void _processStatus(const uint8_t *buffer, const size_t length);

    AsyncUDP _udp;
    UpdateHandlerFunction _updateHandler;
    StatusHandlerFunction _statusHandler;

    Color _colorBuffer[MAXIMUM_COLORS];
};