Instructions for building the solution:

- Switch the solution from Debug mode to Release mode, if it has not been done automatically.
- Whilst all DLLs have already been configured to be built in the correct directories, this configuration
  may not have carried over correctly; for reference:
	* The project SMTPServer needs to be built directly into the Server's bin folder.
	* The project SMTPClient needs to be built directly into the Client's bin folder.
	* The project EchoServer is not necessary, but if testing it is desired then it too needs to be built directly into the Server's bin folder.
- Build the solution through the Build menu, or through the CTRL + SHIFT + B shortcut - note that running the application directly will NOT build all necessary DLLs.
- Copy the data files provided in the Data folder in the Solution's root directory into their respective bin folders. These are required, without them the shells will not open.
- Run one instance of the Server.exe shell. Whilst more than one instance can be ran safely only one can listen on a specific IP/port combination.
- Run as many instances as the Client.exe shell as is desired.

Instructions on how to use the Server shell:

- A server program must be loaded through the server --load command. If the EchoServer project was included, then the choice is between it and SMTPServer. Otherwise, the latter is the
  only option.
- The server can be configured through the vars command. To set a variable, use the vars --set / --value flag.
- The server can then be started through the start command. If no --host is provided, the default one will be chosen from the environment variables. Same goes for the port (--port).

Instructions on how to use the Client shell:

- (Optional) A client command set can be loaded through the load --pack command, the SMTPClient command set comes by default and provides some commands to simplify SMTP operations.
- Using the connect --host / --port command, connect to the address and port of the desired server.
- To send data, either use a command that does that, or use the send --data command to send arbitrary data. Direct mode can be used to communicate with the server directly, to use it
  simply put a dollar sign ($) character before any command.