# a-MOBA-game-server
original article from https://blog.csdn.net/yupu56/article/details/83832971

Original server is a MOBA class game server. Developers can refer it to build other kinds of game servers.

1.Game protocol inculdes: (protocol.cs)
Login protocol, User protocol, Match protocol, Character pick protocol, In-Battle protocol

2.For Data Transmission object: (UserDto.cs)
Transmit User Game Data

3.For Game Character constant object: (CharacterData.cs)
Create character data model

4.Socket Model handle dividing module and sub-module, message encoding and decoding. Socket Model has three layers, the first divides modules, the second divides sub-modules and the third divides current logic function (NetFrameAuto.cs)

5.Serialize byte object in buffer, create server and token pool, process received message, write buff in cache,
judge if message queue has messages. Define abstract class when client connect, client receive messagea and client disconnet. (NetFrame.cs)

6.Handle distribution to certain logic operation in the case of client connect, disconnect and messages receive. (Server.cs)

7.Logic handle module handle main logic operations include login, User handle, match-up, character pick, battle etc.
AbsOnceHanlder handle single unit message send. AbsMultiHandler handle multi messages send. (ServerLogic.cs)

