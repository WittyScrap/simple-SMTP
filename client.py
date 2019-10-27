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
            "CONTINUOUS": True,
            "BUFFER_SIZE": int(2048),
            "EXPECT_WELCOME_MESSAGE": False,
            "USE_SSL": False
        }

    def get_host(self):
        return self.__host if self.__is_connected else ""

    def get_port(self):
        return self.__port if self.__is_connected else 0

    def __show_variables(self, segments):
        return 'Environment variables:\n\n' + '\n'.join(k.lower() + ' = ' + str(self.__ENV_VARS[k]) for k in
                                                        self.__ENV_VARS) + '\n', False

    def send_bytes(self, command: bytes):
        if self.__is_connected:
            try:
                self.__socket.sendall(command)
            except ConnectionAbortedError as e:
                if hasattr(e, "message"):
                    self.__local_message("The connection was aborted due to the following reason:", e.message)
                else:
                    print(self.__local_label + ": The connection was aborted due to the following reason: ", end='')
                    print(e)

    def __parse(self, response):
        return self.__parser.__parse_response__(response)

    def __set_parser_command(self, segments):
        if len(segments) != 2:
            return "Invalid use of the USE command, usage: USE {parser_name | __none}. See HELP for more info.", False

        parser_name = segments[1]

        if parser_name.upper() == "__NONE":
            self.__parser = None
            return "Parser successfully reset.", False

        try:
            parser_module = importlib.import_module(parser_name)

            if not hasattr(parser_module, "__parse_response__"):
                raise Exception("Invalid parser module (entry point not implemented).")
            else:
                self.__parser = parser_module
                return "Parser " + parser_name + " loaded successfully.", False
        except Exception as e:
            return "Error: " + str(e), False

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

    def __recvall(self):
        buffer_size = self.__ENV_VARS["BUFFER_SIZE"]
        if self.__ENV_VARS["CONTINUOUS"]:
            response = bytearray()
            try:
                while True:
                    data = self.__socket.recv(buffer_size)
                    if not data:
                        break
                    else:
                        response.extend(data)
            except socket.timeout as e:
                if response:
                    return response
                else:
                    raise e
        else:
            return self.__socket.recv(buffer_size)

    def __response(self):
        response = self.__recvall()

        if not response:
            return None

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

    def receive_bytes(self):
        try:
            response = self.__response()
            if not response:
                self.disconnect()
            return response
        except socket.timeout:
            print(self.__local_label + ": Response timed out.")

    @staticmethod
    def __get_spaces(count: int) -> str:
        return ''.join(' ' for x in range(count))

    def __handle_input(self) -> bytes:
        entry = input(self.__host + ">> ")
        command = ""

        while entry and entry[-1] == "\\":
            command += entry[:-1] + "\r\n"
            entry = input(self.__get_spaces(len(self.__host)) + ">> ")

        command += entry

        return command.encode()

    @staticmethod
    def __parse_exec_call(call) -> (str, str):
        segments = call.split(b'.')
        if len(segments) == 1:
            return segments[0], None

        function = segments[-1]
        module = b'.'.join(segments[:-1])

        return module, function

    def __exec_script(self, exec_command):
        if len(exec_command) == 1:
            self.__local_message("Incomplete EXEC command: please provide call and arguments (where needed).")
            return

        segments = exec_command[1:]
        call = segments[0]
        args = segments[1:] if len(segments) > 1 else None

        module, function = Shell.__parse_exec_call(call)
        if not function:
            self.__local_message("Incomplete EXEC command: module name or function call missing.")
            return

        try:
            imported_module = importlib.import_module(module.decode())
            imported_method = getattr(imported_module, function.decode())
            imported_method(self, args)
        except Exception as e:
            if hasattr(e, "message"):
                self.__local_message(e.message)
            else:
                print(self.__local_label + ": ", end='')
                print(e)

    def __handle_remote_macros(self, command: bytes) -> bool:
        if command[:2] != b"::":
            return False

        command_structure = command[2:].split()
        command_core = command_structure[0].upper()
        if command_core == b"EXIT":
            self.disconnect()
            return True
        if command_core == b"EXEC":
            self.__exec_script(command_structure)
            return True

        return False

    def __shell(self):
        while self.__is_connected:
            command = self.__handle_input()

            if not self.__handle_remote_macros(command):
                self.send_bytes(command)
                self.receive_bytes()

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

            timeout = self.__ENV_VARS["TIMEOUT"] if not self.__ENV_VARS["BLOCKING"] else None

            self.__socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.__socket.settimeout(timeout)  # Connection timeout
            self.__socket.connect((host, port))
            self.__socket.setblocking(not non_blocking)
            self.__socket.settimeout(timeout)  # Message handling timeout

            if self.__ENV_VARS["USE_SSL"]:
                self.__socket = ssl.wrap_socket(self.__socket, keyfile=None, certfile=None, server_side=False,
                                                cert_reqs=ssl.CERT_NONE, ssl_version=ssl.PROTOCOL_SSLv23)

            self.__is_connected = True

            if self.__ENV_VARS["EXPECT_WELCOME_MESSAGE"]:
                self.receive_bytes()

        except (socket.gaierror, TimeoutError, ssl.SSLError, ConnectionRefusedError) as e:
            return "Could not connect to specified address on specified port due to error: " + str(type(e)) + "."


shell = Shell("local")
shell.start()
