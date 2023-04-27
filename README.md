# UnityMultiplayerARPG_MMO_GuildWar
Guild war for MMORPG KIT (Available for MMO only)

It will connect to [mmorpg-kit-guild-war-service](https://github.com/insthync/mmorpg-kit-guild-war-service). So you have to setup it, try googling how to install Node.js, then follow the [instruction](https://github.com/insthync/mmorpg-kit-guild-war-service/blob/main/README.md).

## Deps
* Require [unity-rest-client](https://github.com/insthync/unity-rest-client)
* Require Newtonsoft.JSON, you can download the one which made for Unity from this https://github.com/jilleJr/Newtonsoft.Json-for-Unity. Or add `"com.unity.nuget.newtonsoft-json": "2.0.0` to `manifest.json` to use `Newtonsoft.JSON` package which made by Unity.

## How to use

After you add this extension to your project, it will have settings that important for guild war service connection there are:

- `guildWarServiceUrl` - Where is the guild war service, if you runs guild war at machine which have public IP is `128.199.78.31` and running on port `9801` (you can set port in `.env` file -> `PORT`), set this to `http://128.199.78.31:9801`
- `guildWarServiceUrlForClient` - It is the same thing with `guildWarServiceUrl` but this one will be sent to clients, and clients will use this value to connect to guild war service.
- `guildWarSecretKey` - Secret key which will be validated at guild war service to allow map-server to use functions. You can set secret keys in `.env` file -> `SECRET_KEYS`, if in the `.env` -> `SECRET_KEYS` value is `["secret1", "secret2", "secret3"]`, you can set this value to `secret1` or `secret2` or `secret3`.
