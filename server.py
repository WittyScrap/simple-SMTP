# Generic server framework

import socket
import selectors
import types


class SocketProtocol:
    TCP = socket.SOCK_STREAM
    UDP = socket.SOCK_DGRAM


class PortRangeException(Exception):
    pass


class Server:
    def __init__(self, host_name, port, protocol):
        self.__socket: socket.socket
        self.__host: str
        self.__port: int

        self.__socket = socket.socket(socket.AF_INET, protocol)
        self.__host = host_name
        self.__port = port

        if port < 0 or port > 65535:
            raise PortRangeException

        # Avoid bind() exception: OSError: [Errno 48] Address already in use
        self.__socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.__socket.bind((host_name, port))

        self.__selector = selectors.DefaultSelector()
        self.__completed = []  # List of sockets that have requested a STOP
        self._on_init()

    def _on_message(self, sock: socket, data: bytes) -> bytes:
        pass

    def _on_init(self):
        pass

    def listen(self):
        self.__socket.listen()
        self.__socket.setblocking(False)
        self.__selector.register(self.__socket, selectors.EVENT_READ | selectors.EVENT_WRITE, data=None)

    def stop(self, sock):
        self.__completed.append(sock)

    def get_host(self):
        return self.__host

    def get_port(self):
        return self.__port

    def __clean(self):
        while self.__completed:
            self.__end(self.__completed.pop())

    def __end(self, sock: socket):
        print("Disconnecting from:", sock.getsockname())
        self.__selector.unregister(sock)
        sock.close()

    def run(self):
        while True:
            events = self.__selector.select(timeout=None)
            for key, mask in events:
                if key.data is None:
                    self.__accept_wrapper(key.fileobj)
                else:
                    self.__service_connection(key, mask)

            self.__clean()

    def __service_connection(self, key, mask):
        sock = key.fileobj
        data = key.data

        if mask & selectors.EVENT_READ:
            try:
                data_received = sock.recv(1024)
                if not data_received:
                    self.stop(sock)
                else:
                    data.outb += self._on_message(sock, data_received)
                    print("[" + ':'.join(map(str, data.addr)) + "] says: " + data_received.decode())
            except (ConnectionAbortedError, ConnectionResetError, ConnectionRefusedError, ConnectionError) as e:
                print("Connection to " + str(sock.getsockname()) + " was abruptly interrupted, closing...")
                self.stop(sock)

        if mask & selectors.EVENT_WRITE and data.outb:
            sent = sock.send(data.outb)
            print("to: " + ':'.join(map(str, data.addr)) + ">> " + data.outb.decode())
            data.outb = data.outb[sent:]

    def __accept_wrapper(self, sock: socket):
        connection, address = sock.accept()  # Should be ready to read
        connection.setblocking(False)
        data = types.SimpleNamespace(addr=address, inb=b'', outb=b'')
        events = selectors.EVENT_READ | selectors.EVENT_WRITE
        self.__selector.register(connection, events, data=data)

        print("Connection accepted from:", address)

        # Send info to client about successful connection
        connection.sendall(b'200:Connection established.')
