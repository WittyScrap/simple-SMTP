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

    def __has_game(self, sock: socket):
        return sock in self.__games

    def __get_players(self):
        players = b""
        for player in self.__games:
            address, port = player.getsockname()
            players += ('\t\t\t- ' + address + ":" + str(port)).encode()

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
    def __is_int(value: bytes):
        try:
            _ = int(value)
        except ValueError:
            return False
        return True

    # Main server loop
    def _on_message(self, sock: socket, data: bytes):
        segments = data.split(b':')

        # Client-sided command structures: [command]:[value] or [command]
        if len(segments) > 2 or len(segments) == 2 and not self.__is_int(segments[1]):
            return b"201:Invalid command structure."

        command = segments[0]
        value = int(segments[1]) if len(segments) == 2 else None

        code, message = self.__handle_command(sock, command, value)
        m_code = bytes(str(code), encoding='utf8')

        if code == GuessingGame.MESSAGE_STOP:
            self.stop(sock)

        return m_code + b':' + message


game = GuessingGame("127.0.0.1", 65432, server.SocketProtocol.TCP)
game.listen()
game.run()
