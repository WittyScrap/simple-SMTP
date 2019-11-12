from encryption import Encryption


# Simple caesar cypher
class Caesar(Encryption):
    # Encrypts a message
    def encrypt(self, message: str, key: int) -> str:
        return ''.join([chr(ord(k) + key) for k in message])

    # Decrypts a message
    def decrypt(self, message: str, key: int) -> str:
        return ''.join([chr(ord(k) - key) for k in message])
