Unchainex Daemon
=============

Unchainex daemon is a _headless_ Unchainex Wallet designed to minimize the usage of resources (CPU, GPU, Memory, Bandwidth) with the goal of
making it more suitable for running all the time in the background.

## Configuration

All configuration options available via `Config.json` file are also available as command line arguments and environment variables:

### Command Line and Environment variables

* Command line switches have the form `--switch_name=value` where _switch_name_ is the same name that is used in the config file (case insensitive).
* Environment variables have the form `UNCHAINEX-SWITCHNAME` where _SWITCHNAME_ is the same name that is used in the config file.

A few examples:

| Config file                | Command line                | Environment variable                |
|----------------------------|-----------------------------|-------------------------------------|
| Network: "TestNet"         | --network=testnet           | UNCHAINEX-NETWORK=testnet           |
| JsonRpcServerEnabled: true | --jsonrpcserverenabled=true | UNCHAINEX-JSONRPCSERVERENABLED=true |
| UseTor: true               | --usetor=true               | UNCHAINEX-USETOR=true               |
| DustThreshold: "0.00005"   | --dustthreshold=0.00005     | UNCHAINEX-DUSTTHRESHOLD=0.00005     |

### Values precedence

* **Values passed by command line arguments** have the highest precedence and override values in environment variables and those specified in config files.
* **Values stored in environment variables** have higher precedence than those in config file and lower precedence than the ones pass by command line.
* **Values stored in config file** have the lower precedence.

### Special values

There are a few special switches that are not present in the `Config.json` file and are only available using command line and/or variable environment:

* **LogLevel** to specify the level of detail used during logging
* **DataDir** to specify the path to the directory used during runtime.
* **BlockOnly** to instruct unchainex to ignore p2p transactions
* **Wallet** to instruct unchainex to open a wallet automatically after started.

### Examples

Run Unchainex and connect to the testnet Bitcoin network with Tor disabled and accept JSON RPC calls. Store everything in `$HOME/temp/unchainex-1`.

```bash
$ unchainex.daemon --usetor=false --datadir="$HOME/temp/unchainex-1" --network=testnet --jsonrpcserverenabled=true --blockonly=true
```

Run Unchainex Daemon and connect to the testnet Bitcoin network.

```bash
$ UNCHAINEX-NETWORK=testnet unchainex.daemon
```

Run Unchainex and open two wallets: AliceWallet and BobWallet

```bash
$ unchainex.daemon --wallet=AliceWallet --wallet=BobWallet
```

### Version

```bash
$ unchainex.daemon --version
Unchainex Daemon 2.0.3.0
```

### Usage

To interact with the daemon, use the [RPC server](https://docs.unchainex.org/using-unchainex/RPC.html) or the [ucli script](https://github.com/Unchainex/Wallet/tree/master/Contrib/CLI).
