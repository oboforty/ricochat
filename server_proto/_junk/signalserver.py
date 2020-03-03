import socket
import threading
from queue import Queue


class ThreadedServer():
    allow_reuse_address = True

    def __init__(self, server_address, num_threads=5):
        self.server_address = server_address
        self.num_threads = num_threads
        self.num_rq_queue = 5

    def serve_forever(self):
        '''
        Handle one request at a time until doomsday.
        '''
        # set up the threadpool
        self.requests = Queue(self.num_threads)
        self.i = 0


        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.bind(self.server_address)
        self.socket.listen(self.num_rq_queue)
        #self.socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        for x in range(self.num_threads):
            t = threading.Thread(target=self.process_request_thread)
            t.setDaemon(True)
            t.start()

        # server main loop
        while True:
            try:
                request = self.socket.accept()
                self.requests.put(request)
            except socket.error as e:
                print("error", e)

        self.socket.close()

    def process_request_thread(self):
        self.i = self.i + 1
        tn = self.i

        while True:
            conn, client_address = self.requests.get()

            while True:
                print("req", self.i, client_address)
                self.handle(conn, client_address)
        conn.close()

class SignalServer(ThreadedServer):
    def __init__(self, server_address):
        super().__init__(server_address)

    def handle(self, request, client_address):
        data = request.recv(1024)
        print(data)

        if data.startswith(b'connect'):
            username = data.split(b'||')[1]
            print(username)
            client_id = self.add_client(client_address, username)

            header = b"ok||" + client_id.to_bytes(8, byteorder="little")

            request.sendall(header)

    def add_client(self, addr, username):
        return 99

        if addr in self.clients:
            # reset username
            client_id = self.clients[addr]
            self.bundle.set_username(client_id, username)
        else:
            # new connection
            client_id, client = self.bundle.create_client(username)
            self.clients[addr] = client_id

        return client_id

if __name__ == "__main__":
    addr = ('0.0.0.0', 9002)
    s = SignalServer(addr)
    print("Listening at ", addr)
    s.serve_forever()
