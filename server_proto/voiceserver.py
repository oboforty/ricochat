import secrets
import socketserver

ENCODING = 'ascii'


class VoiceServer(socketserver.UDPServer):
    allow_reuse_address = True
    max_packet_size = 8192

    def __init__(self, bundle, server_address):
        self.bundle = bundle
        self.echo_server = False
        self.qos = False

        self.connections = {}
        self.uid2addr = {}

        super().__init__(server_address, self.onreceive)

    def onreceive(self, request, client_address, server):
        packet, socket = request

        # todo: temporal signaling here
        if packet.startswith(b'vcon'):
            parts = packet.split(b' ')

            vcid = parts[1].decode(ENCODING)
            uid = parts[2].decode(ENCODING)
            client = self.bundle.clients[uid]

            self.connections[client_address] = (socket, client)
            self.uid2addr[uid] = client_address

            print("VCON:", vcid, client['username'], uid)
            socket.sendto(b"voke", client_address)
        else:
            # voice data - echo
            socket, client = self.connections[client_address]

            if self.echo_server:
                msg = self.wrap_msg(packet, client['vcid'])
                socket.sendto(msg, client_address)
            else:
                # spread voice data
                self.send_channel(client['channel'], client['vcid'], packet)

    def wrap_msg(self, msg, vcid):
        # mask: |-|-|-|-|-|-|-|Q|
        masks = 0
        if self.qos:
            masks |= 1 # Q - qos

        header = vcid.to_bytes(7, byteorder="little")
        header += bytes([masks])

        return header+msg

    # def send(self, msg, addr, qos=False):
    #     socket = self.connections[addr]
    #     socket.sendto(header+msg, addr)

    def send_channel(self, chname, vcid, msg):
        clients = self.bundle.channels[chname]
        msg = self.wrap_msg(msg, vcid)

        for uid in clients:
            addr = self.uid2addr[uid]
            socket, client = self.connections[addr]

            if client['vcid'] == vcid:
               # skip sending user's vc
               continue

            socket.sendto(msg, addr)
