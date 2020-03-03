import socket
import socketserver
from collections import defaultdict


class MyTCPHandler:

    def __init__(self, request, client_address, server):
        try:
            data = request.recv(1024)

            if data.startswith(b'connect'):
                username = data.split(b'||')[1]
                print(username)
                client_id = server.add_client(client_address, username)

                header = b"ok||" + client_id.to_bytes(8, byteorder="little")

                request.sendall(header)
        except Exception as e:
            print(e)

    # def handle(self):
    #     # self.request is the TCP socket connected to the client
    #     self.data = self.request.recv(1024).strip()
    #     print("{} wrote:".format(self.client_address[0]))
    #     print(self.data)
    #     # just send back the same data, but upper-cased
    #     self.request.sendall(self.data.upper())


class SignalingServer:
    #allow_reuse_address = True

    def __init__(self, bundle, server_address):
        self.bundle = bundle

        # address -> client_id
        self.clients = {}
        self.addr = server_address

        #super().__init__(server_address, MyTCPHandler)

    def serve_forever(self):
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.bind(self.addr)
        s.listen(5)

        conn, addr = s.accept()

        while True:
            data = conn.recv(1024)
            conn.send(data)
        #conn.close()

    def add_client(self, addr, username):
        if addr in self.clients:
            # reset username
            client_id = self.clients[addr]
            self.bundle.set_username(client_id, username)
        else:
            # new connection
            client_id, client = self.bundle.create_client(username)
            self.clients[addr] = client_id

        return client_id
