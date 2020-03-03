import secrets
import socketserver

ENCODING = 'ascii'

class MyUDPHandler:

    def __init__(self, request, client_address, server):
        packet, socket = request

        # todo: temporal signaling here
        if packet.startswith(b'vcon'):
            username = packet.split(b' ')[1].decode(ENCODING)
            server.create_client(username, socket, client_address)
        elif packet.startswith(b'join'):
            chname = packet.split(b' ')[1].decode(ENCODING)
            server.switch_channel(chname, client_address)
        else:
            # voice data - echo
            client_id = server.clients[client_address]
            server.send(packet, client_id)

            # spread voice data
            #server.send_channel(client_address, packet)


class VoiceServer(socketserver.UDPServer):
    allow_reuse_address = True
    max_packet_size = 8192

    def __init__(self, bundle, server_address):
        # channel -> client_id
        self.bundle = bundle
        # client_id -> addr, socket
        self.connections = {}
        # addr -> client_id
        self.clients = {}

        super().__init__(server_address, MyUDPHandler)

    def create_client(self, username, socket, addr):
        if addr in self.clients:
            client_id = self.clients[addr]

            # reset username
            self.bundle.set_username(client_id, username)
            client = self.bundle.clients[client_id]

            # set relations
            self.connections[client_id] = (addr, socket)
        else:
            client_id, client = self.bundle.create_client(username)

            # set relations
            self.clients[addr] = client_id
            self.connections[client_id] = (addr, socket)

        print("New client: {}".format(username))
        self.send(b'voke', client_id, qos=True)

        return client

    def switch_channel(self, chname, addr):
        client_id = self.clients[addr]
        client = self.bundle.clients[client_id]
        ex_channel = client['channel']

        # join new one
        if self.bundle.set_channel(client_id, chname):
            if ex_channel:
                # leave current channel
                self.bundle.leave_channel(client_id)
                self.send_channel(ex_channel, b'leav', qos=True)

            # make a string of all users
            str0 = []
            for client_id2 in self.bundle.channels[chname]:
                username = self.bundle.clients[client_id2]['username']
                str0.append(str(client_id2) + ':' + username)

            self.send(('|'.join(str0)).encode(ENCODING), client_id)

            username = client['username']
            print("switched channel", chname, username, client_id, str0)
            #self.send_channel(b'join '+username,chname, qos=True)

    def send(self, msg, client_id, qos=False):
        addr, socket = self.connections[client_id]

        # mask: |-|-|-|-|-|-|-|Q|
        masks = 0
        if qos: masks |= 1 # Q - qos

        header = client_id.to_bytes(7, byteorder="little")
        header += bytes([masks])
        socket.sendto(header+msg, addr)

        if qos:
            # todo: later check if ACKresponse have been given
            pass

    def send_channel(self, msg, chname, qos=False, exception=None):
        clients = self.bundle.channels[chname].copy()

        # multicast to everyone except sending user
        if exception is not None:
            clients.remove(exception)

        for client_id in clients:
            self.send(msg, client_id, qos=qos)
