# ESC1: Multicast.

Sender:

cls && artemis-server-client.exe -batchmode -nographics -clientMode sender -address test.address -interval 2

Receiver:

cls && artemis-server-client.exe -batchmode -nographics -clientMode receiver -address test.address -queue q1 -interval 2 -ack ack


# ESC2: Anycast.

Sender:

cls && artemis-server-client.exe -batchmode -nographics -clientMode sender -address test.address -interval 2

Receiver:

cls && artemis-server-client.exe -batchmode -nographics -clientMode receiver -address test.address -queue q1 -interval 2 -ack ack
cls && artemis-server-client.exe -batchmode -nographics -clientMode receiver -address test.address -queue q1 -interval 2 -ack ack


# ESC3: Wildcards.

Sender:

cls && artemis-server-client.exe -batchmode -nographics -clientMode sender -address user.login.test -interval 2

Receiver:

cls && artemis-server-client.exe -batchmode -nographics -clientMode receiver -address *.login.test -queue q1 -interval 2 -ack ack
cls && artemis-server-client.exe -batchmode -nographics -clientMode receiver -address *.login.* -queue q2 -interval 2 -ack ack
cls && artemis-server-client.exe -batchmode -nographics -clientMode receiver -address #.test -queue q3 -interval 2 -ack ack
