import socket
import socketserver
import threading
from queue import Queue


class ThreadPoolMixIn(socketserver.ThreadingMixIn):
    '''
    use a thread pool instead of a new thread on every request
    '''
    # numThreads = 50

    def process_request_thread(self):
        '''
        obtain request from queue instead of directly from server socket
        '''
        while True:
            socketserver.ThreadingMixIn.process_request_thread(self, *self.requests.get())

    def handle_request(self):
        '''
        simply collect requests and put them on the queue for the workers.
        '''
        try:
            request, client_address = self.get_request()
        except socket.error:
            return
        if self.verify_request(request, client_address):
            self.requests.put((request, client_address))

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
            raise e



class ThreadedTCPServer(ThreadPoolMixIn, socketserver.TCPServer):
    #Extend base class and overide the thread paramter to control the number of threads.
    def __init__(self, server_address):
        self.numThreads = 5

        super().__init__(server_address, MyTCPHandler)



if __name__ == "__main__":
    server_address = ('0.0.0.0', 9002)
    # httpd = ServerClass(server_address, HandlerClass)
    server = ThreadedTCPServer(server_address)
    sa = server.socket.getsockname()

    server.serve_forever()
