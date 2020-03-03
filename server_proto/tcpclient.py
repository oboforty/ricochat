import socket
import threading
import time

TCP_IP = "127.0.0.1"
TCP_PORT = 9002


def do_thread():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 1)
    sock.connect((TCP_IP, TCP_PORT))
    sock.settimeout(30)

    sock.sendall(b'connect||oboforty')
    resp = sock.recv(12)
    client_id = int.from_bytes(resp[4:], byteorder='little')

    if resp[:2] != b'ok':
        print("Error", resp)
    else:
        print("Client id:", client_id)

    sock.sendall(b'hai')
    resp = sock.recv(3)
    print(resp)

    time.sleep(10)

    sock.sendall(b'ha2')
    resp = sock.recv(3)
    print(resp)

    sock.close()


for i in range(20):
    t = threading.Thread(target=do_thread)
    t.start()
