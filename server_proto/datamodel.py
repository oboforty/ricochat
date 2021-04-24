import json
import threading
from collections import defaultdict


class Bundle:
    def __init__(self):
        with open("channels.json") as fh:
            self.channel_info = json.load(fh)

        self.client_id = 0
        self.clock = threading.Lock()

        # uid -> client descriptor relation
        self.clients = {}
        # channel -> uids relation
        self.channels = defaultdict(set)

    def create_client(self, uid, username, chname=None):
        self.clock.acquire()
        self.client_id += 1
        vcid = self.client_id
        self.clock.release()

        client = {
            "uid": uid,
            "vcid": vcid,
            "username": username,
            "channel": None,
            "about": "",
            "role": "user",
            "speaking": False
        }

        self.clients[uid] = client

        if chname:
            self.set_channel(uid, chname)

        return client

    def set_username(self, uid, username):
        if uid not in self.clients:
            return False

        self.clients[uid]['username'] = username

    def set_channel(self, uid, chname):
        if chname not in self.channel_info:
            return False

        if uid not in self.clients:
            return False

        if self.clients[uid]['channel']:
            # you have to request to leave channel first
            return False

        self.clients[uid]['channel'] = chname
        self.channels[chname].add(uid)

        return True

    def leave_channel(self, uid):
        if uid not in self.clients:
            return False

        exch = self.clients[uid]['channel']
        if not exch:
            return False

        self.channels[exch].remove(uid)
        self.clients[uid]['channel'] = None

        return exch
