import secrets
import socketserver
import threading

ENCODING = 'ascii'


class HoneyPotServer(socketserver.UDPServer):
    allow_reuse_address = True
    max_packet_size = 8192

    def __init__(self, server_address):
        self.echo_server = False
        self.qos = False

        self.connections = {}
        self.uid2addr = {}

        super().__init__(server_address, self.onreceive)

    def onreceive(self, request, client_address, server):
        packet, socket = request

        print(packet, 'from', client_address)


if __name__ == "__main__":
    ADDR = "0.0.0.0"
    PORT = 9001

    def thread_udp():
        _port = PORT
        print("RicoChat Voice server: listening on {}:{}".format(ADDR, _port))
        serverUDP = HoneyPotServer((ADDR, _port))
        serverUDP.serve_forever()

    t2 = threading.Thread(target=thread_udp)
    t2.start()
