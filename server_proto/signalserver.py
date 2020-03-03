import socket
import selectors
import types


class SignalingServer:
    def __init__(self, bundle, addr):
        self.bundle = bundle
        self.sel = selectors.DefaultSelector()

        self.clients = {}

        lsock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        lsock.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 1)
        lsock.bind(addr)
        lsock.listen()

        lsock.setblocking(False)
        self.sel.register(lsock, selectors.EVENT_READ, data=None)

        events = selectors.EVENT_READ | selectors.EVENT_WRITE

    def accept_wrapper(self, sock):
        conn, addr = sock.accept()  # Should be ready to read
        conn.setblocking(False)

        # register connection
        data = types.SimpleNamespace(addr=addr, inb=b'', outb=b'')
        events = selectors.EVENT_READ | selectors.EVENT_WRITE
        self.sel.register(conn, events, data=data)

    def service_connection(self, key, mask):
        sock = key.fileobj
        addr = key.data.addr

        if mask & selectors.EVENT_READ:
            data = sock.recv(1024)

            if data:
                if data.startswith(b'connect'):
                    # authenticate
                    username = data.split(b'||')[1]
                    client_id = self.add_client(addr, username)
                    print('new connection:', client_id, username, addr)

                    header = b"ok||" + client_id.to_bytes(8, byteorder="little")
                    key.data.outb += header
                else:
                    # echo server
                    key.data.outb += data
            else:
                # todo: later close connection after a bit of unconnectivity
                print('closing connection to', addr)
                self.sel.unregister(sock)
                #sock.close()
                pass

        if mask & selectors.EVENT_WRITE:
            outb = key.data.outb
            if outb:
                # forwarding repr(data.outb) to data.addr
                sent = sock.send(outb)
                key.data.outb = outb[sent:]

    def serve_forever(self):
        while True:
            events = self.sel.select(timeout=None)
            for key, mask in events:
                if key.data is None:
                    self.accept_wrapper(key.fileobj)
                else:
                    self.service_connection(key, mask)

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
