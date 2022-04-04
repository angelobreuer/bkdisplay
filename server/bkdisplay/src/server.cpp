#include "server.hpp"

void BKServer::begin()
{
    _udp.listen(8996);
    _udp.onPacket(_handlePacketStatic, this);
    printf("server ok\n");
}

void BKServer::_handlePacketStatic(void *arg, AsyncUDPPacket &packet)
{
    printf("packet recv %d.\n", packet.length());
    reinterpret_cast<BKServer *>(arg)->_handlePacket(packet);
}

void BKServer::_processUpdate(const uint8_t *buffer, const size_t length)
{
    if (!_updateHandler || length < 2)
    {
        return;
    }

    uint8_t colorTableLength = buffer[0];
    uint8_t dataLength = buffer[1];

    if (length < 2 + colorTableLength * 3 + dataLength)
    {
        return;
    }

    const Color *colorTable = reinterpret_cast<const Color *>(buffer + 2);
    const uint8_t *data = buffer + 2 + colorTableLength * 3;

    for (int index = 0; index < dataLength; index++)
    {
        uint8_t colorIndex = data[index];

        if (colorIndex >= colorTableLength)
        {
            return; // invalid color index
        }

        _colorBuffer[index] = colorTable[colorIndex];
    }

    _updateHandler(_colorBuffer, dataLength);
}

void BKServer::_processStatus(const uint8_t *buffer, const size_t length)
{
    if (length != 1)
    {
        return;
    }

    if (_statusHandler)
    {
        _statusHandler(!!*buffer);
    }
}

void BKServer::_handlePacket(AsyncUDPPacket &packet)
{
    // All payloads contain at least 2 bytes
    if (packet.available() < 2)
    {
        return;
    }

    const uint8_t *buffer = packet.data();
    uint8_t opCode = *buffer;

    // Only op codes 0 and 1 exist
    if (opCode == 0)
    {
        _processStatus(packet.data()+1, packet.length()-1);
    }
    else if (opCode == 1)
    {
        _processUpdate(packet.data()+1, packet.length()-1);
    }
}

void BKServer::onUpdate(const UpdateHandlerFunction handler)
{
    _updateHandler = handler;
}

void BKServer::onStatus(const StatusHandlerFunction handler)
{
    _statusHandler = handler;
}