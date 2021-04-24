import threading

from datamodel import Bundle
from signalserver import SignalingServer
from voiceserver import VoiceServer


if __name__ == "__main__":
    ADDR = "0.0.0.0"
    PORT = 9001

    print("RicoChat loading server")

    bundle = Bundle()

    def thread_tcp():
        _port = PORT + 1
        print("RicoChat Signal server: listening on {}:{}".format(ADDR, _port))
        serverTCP = SignalingServer(bundle, (ADDR, _port))
        serverTCP.serve_forever()

    def thread_udp():
        _port = PORT
        print("RicoChat Voice server: listening on {}:{}".format(ADDR, _port))
        serverUDP = VoiceServer(bundle, (ADDR, _port))
        serverUDP.serve_forever()

    t1 = threading.Thread(target=thread_tcp)
    t1.start()

    t2 = threading.Thread(target=thread_udp)
    t2.start()
