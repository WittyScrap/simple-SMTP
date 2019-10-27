def get(shell, args: list):
    if args and type(args) is list and len(args) == 1:
        shell.send_bytes(b"GET " + args[0] + b" HTTP/1.1\r\nHost: " + shell.get_host().encode() + b"\r\n\r\n")
        shell.receive_bytes()
    else:
        raise Exception("Invalid argument count!")


def head(shell, args: list):
    if args and type(args) is list and len(args) == 1:
        shell.send_bytes(b"HEAD " + args[0] + b" HTTP/1.1\r\nHost: " + shell.get_host().encode() + b"\r\n\r\n")
        shell.receive_bytes()
    else:
        raise Exception("Invalid argument count!")


def options(shell, args: list):
    if args and type(args) is list and len(args) == 1:
        shell.send_bytes(b"OPTIONS " + args[0] + b" HTTP/1.1\r\nHost: " + shell.get_host().encode() + b"\r\n\r\n")
    else:
        shell.send_bytes(b"OPTIONS * HTTP/1.1\r\nHost: " + shell.get_host().encode() + b"\r\n\r\n")
    shell.receive_bytes()
