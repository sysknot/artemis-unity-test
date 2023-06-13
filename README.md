# ESC1:
Multicast.

- Sender:
* cls && artemis-server-client.exe -batchmode -nographics 1 5 multicast nondurable user.login.test

- Receiver:
* cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable user.login.test

# ESC2:
Anycast.

- Sender:
* cls && artemis-server-client.exe -batchmode -nographics 1 5 anycast nondurable user.login.test2

- Receiver:
* cls && artemis-server-client.exe -batchmode -nographics 2 2 anycast nondurable user.login.test2

# ESC3:
Wildcards.

- Sender:
* cls && artemis-server-client.exe -batchmode -nographics 1 5 multicast nondurable user.login.test

- Receiver:
* cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable *.login.test
* cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable *.login.*
* cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable #.test
