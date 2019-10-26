# Client side for any server.Server based application

import socket
import importlib
import os
import ssl


class Shell:
    def __init__(self, shell_label: str):
        self.__host: str = ""
        self.__port: int = 0
        self.__local_label = shell_label
        self.__socket = None
        self.__is_connected = False
        self.__parser = None

        self.__COMMANDS = {
            "CONNECT": self.__connect_command,
            "SET": self.__set_command,
            "VARS": self.__show_variables,
            "EXIT": lambda x: ("Goodbye", True),
            "HELP": self.__show_help,
            "USE": self.__set_parser_command,
            "PARSER": lambda x: (self.__parser.__name__, False) if self.__parser else ("No parser.", False)
        }

        self.__ERROR_LEVELS = {
            0: "OK",    # No problems detected
            1: "ER",    # Error reported
            2: "ES"     # Exit session
        }

        self.__ENV_VARS = {
            "TIMEOUT": 5.0,
            "BLOCKING": False,
            "RAW_INPUT": False,
            "BUFFER_SIZE": int(2048),
            "EXPECT_WELCOME_MESSAGE": True,
            "USE_SSL": False
        }

    def __show_variables(self, segments):
        return 'Environment variables:\n\n' + '\n'.join(k.lower() + ' = ' + str(self.__ENV_VARS[k]) for k in
                                                        self.__ENV_VARS) + '\n', False

    def send_command(self, command: bytes):
        self.__socket.sendall(command)

    def __parse(self, response):
        return self.__parser.__parse_response__(response)

    def __set_parser_command(self, segments):
        if len(segments) != 2:
            return "Invalid use of the USE command, usage: USE {parser_name | __none}. See HELP for more info.", False

        parser_name = segments[1]

        if parser_name.upper() == "__NONE":
            self.__parser = None
            return "Parser successfully reset.", False

        if not os.path.exists(parser_name + ".py"):
            return "Module not found (Are you missing a component for the client?).", False

        try:
            parser_module = importlib.import_module(parser_name)
            self.__parser = parser_module

            return "Parser " + parser_name + " loaded successfully.", False
        except Exception as e:
            return "Unable to load parser module due to exception: " + str(e), False

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

    # error_visibility: 0 = never show error level, 1 = only show errors, 2 = show all.
    def __display(self, error_level: int, response: bytes, error_visibility=2):
        print("[" + self.__host + (":" + self.__ERROR_LEVELS[error_level] if error_visibility == 2 or error_visibility
                                                                             == 1 and error_level > 1 else "") + "] " +
              response.decode())

    def __response(self):
        response = self.__socket.recv(self.__ENV_VARS["BUFFER_SIZE"])

        if self.__parser:
            msg, err_level = self.__parse(response)
            self.__display(err_level, msg)
            return err_level < 2
        else:
            self.__display(0, response, 1)
            return response

    def disconnect(self):
        self.__is_connected = False
        self.__socket.close()

    def __handle_response(self):
        try:
            if not self.__response():
                self.disconnect()
        except socket.timeout:
            print(self.__local_label + ": Response timed out.")

    def __shell(self):
        while self.__is_connected:
            command = input(self.__host + ">> ").encode()

            if self.__ENV_VARS["RAW_INPUT"]:
                command = self.__unsanitize(command)

            if command.upper() == b"::EXIT":
                self.disconnect()
            else:
                self.send_command(command)
                self.__handle_response()

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
        return "Commands:\n\n" \
               "CONNECT <address> <port>     - Connects to a specific host.\n" \
               "SET <var_name> <float_value> - Sets a variable. For a list of variables, use VARS.\n" \
               "VARS                         - Lists all environment variables.\n" \
               "EXIT                         - Exits the client\n" \
               "HELP                         - Shows this help message\n" \
               "USE {parser_name | __none}   - Selects the message parser. This must be a python module.\n" \
               "PARSER                       - Shows the current parser.\n", False

    def __connect_command(self, segments) -> (str, bool):
        if len(segments) != 3:
            return "Invalid use of CONNECT command, format: CONNECT <address> <port> {non blocking:True/False}", False

        if not self.__is_int(segments[2]) or int(segments[2]) < 0 or int(segments[2]) > 65535:
            return "Port must be a number from 0 to 65535.", False

        o = self.connect(segments[1], int(segments[2]), not self.__ENV_VARS["BLOCKING"], True)

        if not o:
            return "Connection to " + segments[1] + ":" + segments[2] + " completed.", False
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

    # I am a funny person yes
    @staticmethod
    def __unsanitize(raw_input: bytes):
        return raw_input.replace(b'\\n', b'\n').replace(b'\\r', b'\r').replace(b'\\t', b'\t')

    def __local_message(self, msg: str):
        print(self.__local_label + ": " + msg)

    def __local_shell(self):
        while not self.__is_connected:
            command = input(self.__local_label + ">> ")

            if command:
                segments = self.__sanitize(command.split())
                output, should_exit = self.__parse_command(segments)

                self.__local_message(output)

                if should_exit:
                    return

        self.__shell()

    def start(self):
        self.__local_shell()

    def connect(self, host: str, port: int, non_blocking=False, show_message=False):
        try:
            self.__host = host
            self.__port = port

            if show_message:
                self.__local_message("Will connect to host: " + host + ", on port: " + str(port))

            self.__socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.__socket.settimeout(self.__ENV_VARS["TIMEOUT"])  # Connection timeout
            self.__socket.connect((host, port))
            self.__socket.setblocking(not non_blocking)
            self.__socket.settimeout(self.__ENV_VARS["TIMEOUT"])  # Message handling timeout

            if self.__ENV_VARS["USE_SSL"]:
                self.__socket = ssl.wrap_socket(self.__socket, keyfile=None, certfile=None, server_side=False,
                                                cert_reqs=ssl.CERT_NONE, ssl_version=ssl.PROTOCOL_SSLv23)

            self.__is_connected = True

            if self.__ENV_VARS["EXPECT_WELCOME_MESSAGE"]:
                self.__handle_response()

        except (socket.gaierror, TimeoutError, ssl.SSLError, ConnectionRefusedError) as e:
            return "Could not connect to specified address on specified port due to error: " + str(type(e)) + "."


shell = Shell("local")
shell.start()
