# Client side for any server.Server based application

import socket


class Shell:
    def __init__(self, shell_label: str):
        self.__host: str = ""
        self.__port: int = 0
        self.__local_label = shell_label
        self.__socket = None
        self.__is_connected = False

        self.__COMMANDS = {
            "CONNECT": self.__connect_command,
            "EXIT": lambda x: ("Goodbye", True),
            "HELP": self.__show_help
        }

    def send_command(self, command: bytes):
        self.__socket.sendall(command)

    def __response(self):
        response = self.__socket.recv(1024)
        segments = response.split(b':')

        code = int(segments[0])
        del segments[0]

        msg = b':'.join(segments)

        return code, msg

    @staticmethod
    def __clean(segment: list):
        return list(filter(None, segment))

    def __shell(self):
        while self.__is_connected:
            code, msg = self.__response()
            print("[" + self.__host + "] " + ("ERROR: " if code == 201 else "") + msg.decode())

            if code == 400:
                self.__is_connected = False
                self.__socket.close()
                continue

            command = None
            while not command:
                command = input(self.__host + ">> ")

            command = command.upper().encode()
            segment = Shell.__clean(command.split())

            if len(segment) > 1:
                svr_command = b':'.join(segment)
                self.send_command(svr_command)
            else:
                self.send_command(command)

        self.__local_shell()

    @staticmethod
    def __is_int(value: bytes):
        try:
            _ = int(value)
        except ValueError:
            return False
        return True

    @staticmethod
    def __show_help(segments) -> (str, bool):
        return "Commands:\n" \
               "CONNECT <address> <port> {non blocking:True/False} - Connects to a specific host.\n" \
               "EXIT - Exits the client\n" \
               "HELP - Shows this help message", False

    def __connect_command(self, segments) -> (str, bool):
        if len(segments) != 3:
            return "Invalid use of CONNECT command, format: CONNECT <address> <port> {non blocking:True/False}", False

        if not self.__is_int(segments[2]) or int(segments[2]) < 0 or int(segments[2]) > 65535:
            return "Port must be a number from 0 to 65535.", False

        o = self.connect(segments[1], int(segments[2]))

        if not o:
            return "Will connect to host: " + segments[1] + ", on port: " + segments[2], False
        else:
            return o, False

    def __parse_command(self, segments) -> (str, bool):
        header = segments[0].upper()

        if header and header in self.__COMMANDS:
            return self.__COMMANDS[header](segments)

        return "Invalid command: " + header, False

    def __local_shell(self):
        while not self.__is_connected:
            command = input(self.__local_label + ">> ")

            if command:
                segments = self.__clean(command.split())
                output, should_exit = self.__parse_command(segments)

                print(self.__local_label + ": " + output)

                if should_exit:
                    return

        self.__shell()

    def start(self):
        self.__local_shell()

    def connect(self, host: str, port: int):
        try:
            self.__host = host
            self.__port = port

            self.__socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.__socket.connect((host, port))

            self.__is_connected = True

        except socket.gaierror:
            return "Could not connect to specified address on specified port."


shell = Shell("local")
shell.start()
