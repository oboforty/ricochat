import secrets
import socketserver
import threading

ENCODING = 'ascii'


class SignalingServer(socketserver.ThreadingTCPServer):
    allow_reuse_address = True
    max_packet_size = 2048

    def __init__(self, bundle, server_address):
        self.bundle = bundle
        # uid -> addr, socket
        self.connections = {}
        # uid -> client descriptor
        self.addr2uid = {}

        super().__init__(server_address, self.onconnect)

    def create_client(self, uid, username, socket, addr):
        client = self.bundle.create_client(uid, username)

        # set relations
        self.addr2uid[addr] = uid
        self.connections[uid] = (addr, socket)

        self.send_to(uid, b'soke '+str(client['vcid']).encode(ENCODING))
        return client

    def switch_channel(self, chname, addr):
        uid = self.addr2uid[addr]
        client = self.bundle.clients[uid]
        ex_channel = client['channel']

        # join new one
        if self.bundle.set_channel(uid, chname):
            if ex_channel:
                # leave current channel
                self.bundle.leave_channel(uid)
                self.send_channel(ex_channel, b'leav', qos=True)

            # make a string of all users
            str0 = []
            for uid2 in self.bundle.channels[chname]:
                client2 = self.bundle.clients[uid2]
                str0.append(str(client2['vcid']) + ':' + client2['username'])

            self.send_to(uid, '|'.join(str0))

            username = client['username']
            vcid = client['vcid']
            print("JOIN:", chname, username, uid)
            self.send_channel(chname, f'join {vcid} {username}'.encode(ENCODING), qos=True)

    def onconnect(self, socket, client_address, server):
        BUFFER = 2048

        while 1:
            # get input with wait if no data
            data = socket.recv(BUFFER)

            # suspect many more data (try to get all - without stop if no data)
            if len(data) == BUFFER:
                while 1:
                    try:  # error means no more data
                        data += socket.recv(BUFFER, socket.MSG_DONTWAIT)
                    except:
                        break

            # no data found exit loop (posible closed socket)
            if data == "":
                break

            # processing input
            data = data.strip()

            if data.startswith(b'scon'):
                parts = data.split(b' ')
                uid = parts[1].decode(ENCODING)
                username = parts[2].decode(ENCODING)

                print(f"SCON: {client_address[0]} {username} {uid} ({threading.currentThread().getName()})")
                self.create_client(uid, username, socket, client_address)
            elif data.startswith(b'join'):
                chname = data.split(b' ')[1].decode(ENCODING)

                self.switch_channel(chname, client_address)
            else:
                # todo: ?idk
                print("Unknown packet: " + data)

    def send_channel(self, chname, msg, qos=False, exception=None):
        addr2uid = self.bundle.channels[chname].copy()

        # multicast to everyone except sending user
        if exception is not None:
            addr2uid.remove(exception)

        if isinstance(msg, str):
            msg = msg.encode(ENCODING)

        for uid in addr2uid:
            self.send_to(uid, msg)

    def send_to(self, uid, msg):
        addr, socket = self.connections.get(uid, (None, None))

        if isinstance(msg, str):
            msg = msg.encode(ENCODING)

        if addr is not None:
            socket.sendto(msg, addr)
