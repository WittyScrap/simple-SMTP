# Guessing game server

import server
import random
import socket


class GuessingGame(server.Server):
    MESSAGE_ERROR = 201
    MESSAGE_OK = 200
    MESSAGE_STOP = 400

    def _on_init(self):
        self.__games = {}  # Dictionary containing all the running games
        print("Running server on: " + self.get_host() + ":" + str(self.get_port()))

    def __has_game(self, sock: socket):
        return sock in self.__games

    def __get_players(self):
        players = b""
        for player in self.__games:
            address, port = player.getpeername()
            players += b'\r\n' + ('\t\t\t- ' + address + ":" + str(port)).encode()

        return players

    def __handle_command(self, sock: socket, command: bytes, value: int) -> (int, bytes):
        if command == b"QUIT":
            if sock in self.__games:
                del self.__games[sock]
            return GuessingGame.MESSAGE_STOP, b"Goodbye!"

        if command == b"INFO" or command == b"HELP":
            return GuessingGame.MESSAGE_OK, b"Welcome to the Guessing game!\n"\
                                            b"Type START to begin playing, then type GUESS and a number to make a "\
                                            b"guess, after which I will tell you how you've fared. If you win, "\
                                            b"you get nothing. You can type PLAYERS at any time to see how many other" \
                                            b"players are connected to the game. Have fun!"

        if command == b"PLAYERS":
            return GuessingGame.MESSAGE_OK, b"Players:\n" + self.__get_players()

        if not self.__has_game(sock):
            if command == b"START":
                self.__games[sock] = random.randint(0, 100)
                return GuessingGame.MESSAGE_OK, b"Ok."

            return GuessingGame.MESSAGE_ERROR, b"Game not started. (INFO/HELP for more info)."

        else:
            if command == b"GUESS" and value:
                if value < self.__games[sock]:
                    return GuessingGame.MESSAGE_OK, b"You guessed too low."
                if value > self.__games[sock]:
                    return GuessingGame.MESSAGE_OK, b"You guessed too high."
                if value == self.__games[sock]:
                    del self.__games[sock]
                    return GuessingGame.MESSAGE_OK, b"Congratulations, you won a meaningless game."

            return GuessingGame.MESSAGE_ERROR, b"Invalid command."

    @staticmethod
    def is_int(value):
        try:
            _ = int(value)
        except ValueError:
            return False
        return True

    # Bare-bones system
    @staticmethod
    def __sanitize(segment: list):
        return list(filter(None, segment))

    # Main server loop
    def _on_message(self, sock: socket, data: bytes):
        segments = GuessingGame.__sanitize(data.upper().split())

        # Client-sided command structures: [command]:[value] or [command]
        if len(segments) > 2 or len(segments) == 2 and not self.is_int(segments[1]):
            return b"201:Invalid command structure."

        command = segments[0]
        value = int(segments[1]) if len(segments) == 2 else None

        code, message = self.__handle_command(sock, command, value)
        m_code = bytes(str(code), encoding='utf8')

        if code == GuessingGame.MESSAGE_STOP:
            self.stop(sock)

        return m_code + b':' + message


config_host = input("HOST: ")
config_host = config_host if config_host else "127.0.0.1"

config_port = input("PORT: ")
config_port = int(config_port) if config_port and GuessingGame.is_int(config_port) else 42069

game = None

try:
    game = GuessingGame(config_host, config_port, server.SocketProtocol.TCP)
except server.PortRangeException:
    print("E: port number must be between 0 and 65535.")
    exit(1)

game.listen()
game.run()
