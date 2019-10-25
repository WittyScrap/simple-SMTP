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
            "SET": self.__set_command,
            "VARS": lambda x: ('Vars:\n'+'\n'.join(k + '=' + str(self.__ENV_VARS[k]) for k in self.__ENV_VARS), False),
            "EXIT": lambda x: ("Goodbye", True),
            "HELP": self.__show_help
        }

        self.__ENV_VARS = {
            "TIMEOUT": 5.0,
            "BLOCKING": False
        }

    def send_command(self, command: bytes):
        self.__socket.sendall(command)

    @staticmethod
    def __parse_type(value: str, src: type):
        bool_values = {
            False: {"FALSE", "0", "N", "NO"},
            True: {"TRUE", "1", "Y", "YES"}
        }

        value = value.upper()

        if src is bool:
            if value in bool_values[False] or value in bool_values[True]:
                return value in bool_values[True]
            else:
                raise ValueError()

        return src(value)

    def __set_command(self, segments):
        if len(segments) != 3:
            return "Invalid use of SET, usage: SET <var_name> <float_value>. See HELP for more info.", False

        var = segments[1].upper()
        value = segments[2]
        if var not in self.__ENV_VARS:
            return "Variable not found.", False

        try:
            self.__ENV_VARS[var] = self.__parse_type(value, type(self.__ENV_VARS[var]))
            return var + " set to: " + str(self.__ENV_VARS[var]), False
        except (TypeError, ValueError):
            return var + " cannot be set to " + value + " because " + var + " is not the same " \
                   "type as " + value + " (" + var + " is " + str(type(self.__ENV_VARS[var])) + \
                   ", but " + value + " is " + str(type(value)) + ").", False

    def __response(self):
        response = self.__socket.recv(1024)
        segments = response.split(b':')

        code = int(segments[0])
        del segments[0]

        msg = b':'.join(segments)

        return code, msg

    def __shell(self):
        while self.__is_connected:
            try:
                code, msg = self.__response()
                print("[" + self.__host + "] " + ("ERROR: " if code == 201 else "") + msg.decode())

                if code == 400:
                    self.__is_connected = False
                    self.__socket.close()
                    continue
            except socket.timeout:
                print(self.__local_label + ": Response timed out.")

            command = input(self.__host + ">> ").encode()

            if command.upper() == b"::EXIT":
                self.__is_connected = False
                self.__socket.close()
                continue

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
    def __show_help(_) -> (str, bool):
        return "Commands:\n" \
               "CONNECT <address> <port>     - Connects to a specific host.\n" \
               "SET <var_name> <float_value> - Sets a variable. For a list of variables, use VARS.\n" \
               "VARS                         - Lists all environment variables.\n" \
               "EXIT                         - Exits the client\n" \
               "HELP                         - Shows this help message", False

    def __connect_command(self, segments) -> (str, bool):
        if len(segments) != 3:
            return "Invalid use of CONNECT command, format: CONNECT <address> <port> {non blocking:True/False}", False

        if not self.__is_int(segments[2]) or int(segments[2]) < 0 or int(segments[2]) > 65535:
            return "Port must be a number from 0 to 65535.", False

        o = self.connect(segments[1], int(segments[2]), not self.__ENV_VARS["BLOCKING"])

        if not o:
            return "Will connect to host: " + segments[1] + ", on port: " + segments[2], False
        else:
            return o, False

    def __parse_command(self, segments) -> (str, bool):
        header = segments[0].upper()

        if header and header in self.__COMMANDS:
            return self.__COMMANDS[header](segments)

        return "Invalid command: " + header, False

    # Bare-bones system
    @staticmethod
    def __sanitize(segment: list):
        return list(filter(None, segment))

    def __local_shell(self):
        while not self.__is_connected:
            command = input(self.__local_label + ">> ")

            if command:
                segments = self.__sanitize(command.split())
                output, should_exit = self.__parse_command(segments)

                print(self.__local_label + ": " + output)

                if should_exit:
                    return

        self.__shell()

    def start(self):
        self.__local_shell()

    def connect(self, host: str, port: int, non_blocking=False):
        try:
            self.__host = host
            self.__port = port

            self.__socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.__socket.connect((host, port))
            self.__socket.setblocking(not non_blocking)
            self.__socket.settimeout(self.__ENV_VARS["TIMEOUT"])

            self.__is_connected = True

        except socket.gaierror:
            return "Could not connect to specified address on specified port."


shell = Shell("local")
shell.start()
