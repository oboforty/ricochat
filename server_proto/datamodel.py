import json
import threading
from collections import defaultdict


class Bundle:
    def __init__(self):
        with open("channels.json") as fh:
            self.channel_info = json.load(fh)

        self.client_id = 0
        self.client_info = {}
        self.client_lock = threading.Lock()

        # client_id -> channel relation
        self.clients = {}
        # channel -> client_ids relation
        self.channels = defaultdict(set)

    def create_client(self, username, chname=None):
        self.client_lock.acquire()
        self.client_id += 1
        client_id = self.client_id
        self.client_lock.release()

        client = {
            "username": username,
            "channel": None,
            "about": "",
            "role": "user",
            "speaking": False
        }

        self.clients[client_id] = client

        if chname:
            self.set_channel(client_id, chname)

        return client_id, client

    def set_username(self, client_id, username):
        if client_id not in self.clients:
            return False

        self.clients[client_id]['username'] = username

    def set_channel(self, client_id, chname):
        if chname not in self.channel_info:
            return False

        if client_id not in self.clients:
            return False

        if self.clients[client_id]['channel']:
            # you have to request to leave channel first
            return False

        self.clients[client_id]['channel'] = chname
        self.channels[chname].add(client_id)

        return True

    def leave_channel(self, client_id):
        if client_id not in self.clients:
            return False

        exch = self.clients[client_id]['channel']
        if not exch:
            return False

        self.channels[exch].remove(client_id)
        self.clients[client_id]['channel'] = None

        return exch
