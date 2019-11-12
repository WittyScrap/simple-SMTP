from abc import ABC, abstractmethod


#
# Encryption base class
#
class Encryption(ABC):
    @abstractmethod
    def encrypt(self, message: str, key: str) -> str:
        pass

    @abstractmethod
    def decrypt(self, message: str, key: str) -> str:
        pass
