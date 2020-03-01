import secrets
import socketserver


class MyUDPHandler:

    def __init__(self, request, client_address, server):
        packet, socket = request

        if packet.startswith(b'vcon'):
            username = packet.split(b' ')[1]
            server.create_client(username.decode('utf-8'), socket, client_address)
            print("New client: {}".format(username))

            server.send_to(client_address, b'voke')
        else:
            # voice data - echo
            # server.send_to(client_address, packet)

            # spread voice data
            server.send_to_channel(client_address, packet)


class VoiceServer(socketserver.UDPServer):
    allow_reuse_address = True
    max_packet_size = 8192

    def __init__(self, server_address):
        self.clients = {}
        self.client_id = 0
        self.default_channel = "CH0"
        self.channels = {
            self.default_channel: {
                "name": "Sakira Fan Club",
                "vce": [8000, 16, 2],
                "protocol": "udp_simple",
                "addr": set()
            }
        }

        super().__init__(server_address, MyUDPHandler)

    def create_client(self, username, socket, addr):
        self.client_id += 1

        client = {
            "username": username,
            "channel": self.default_channel,
            "sock": socket,
            "id": self.client_id
        }
        self.clients[addr] = client
        self.channels[client['channel']]['addr'].add(addr)

        return client

    def send_to(self, addr, msg):
        client = self.clients[addr]

        header = client['id'].to_bytes(8, byteorder="little")
        client['sock'].sendto(header+msg, addr)

    def send_to_channel(self, addr, msg):
        client = self.clients[addr]
        channel = self.channels[client['channel']]

        # multicast to everyone except sending user
        for addr2 in channel['addr']:
            if addr2 != addr:
                self.send_to(addr2, msg)


if __name__ == "__main__":
    ADDR = "0.0.0.0"
    PORT = 9001

    print("RicoChat: listening on {}:{}".format(ADDR, PORT))
    serverUDP = VoiceServer((ADDR, PORT))
    serverUDP.serve_forever()
