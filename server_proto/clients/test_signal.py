import socket
import sys

addr = ('localhost', 9002)

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

s.connect(addr)
print("Connected")
#data = s.recv(64)
#print(f'{s.getsockname()}: received {data}')

s.send(b'scon abcdefg')
data = s.recv(64)
print(f'{s.getsockname()}: received {data}')

s.close()
