import socket
import threading

#UDP_IP = "127.0.0.1"
import time

UDP_IP = "80.217.114.215"
UDP_PORT = 9001




sock = socket.socket(socket.AF_INET,  socket.SOCK_DGRAM)

#sock.connect((UDP_IP, UDP_PORT))
sock.sendto(b'vcon oboforty', (UDP_IP, UDP_PORT))

resp = sock.recv(1024)
print(resp)


def worker():
    """thread worker function"""

    while True:
        b2 = sock.recv(1024)

        print(int.from_bytes(b2[:8], byteorder='little'), b2[8:])

t = threading.Thread(target=worker)
t.start()


def worker2():
    while True:
        sock.sendto(b'thank you lord jesus', (UDP_IP, UDP_PORT))

        time.sleep(4)

t = threading.Thread(target=worker2)
t.start()
