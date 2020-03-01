import socket
import logging
import threading
import time
import queue

queue_out = queue.SimpleQueue()
queue_in = queue.SimpleQueue()

UDP_IP = "127.0.0.1"
UDP_PORT = 5005
UDP_PORT = 5005


class RicoServer:

    def __init__(self):
        pass



def receiving_thread(name):
    #time.sleep(2)

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((UDP_IP, UDP_PORT))

    while True:
        data, addr = sock.recvfrom(1024)


def sending_thread(name):
    #time.sleep(2)
    sock = socket.socket(socket.AF_INET,  # Internet
                         socket.SOCK_DGRAM)  # UDP
    sock.sendto(MESSAGE, (UDP_IP, UDP_PORT))
