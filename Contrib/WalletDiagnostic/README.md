Wallet diagnostic
----------

Toolset for observe the internal state of wallets.

## Unchainex keys state generation

Create a visual representation from a Unchainex keys dump.

### How to run it

Run Unchainex, open the wallet you are interested in.

Next open a terminal and enter:

```bash
dotnet fsi keygraph.fsx <wallet-name>
```

### The result

![walletkeys.png](img/walletkeys.png)

### Dependencies

None

## Unchainex CoinGraph generation

Create a visual representation from a Unchainex coins dump.

### How to run it

Run Unchainex, open the wallet you are interested in.

Next open a terminal and enter:

```bash
dotnet fsi txgraph.fsx <wallet-name> <initial-txid> | dot -Tpng | feh  -
```

## The result

![txgraph.png](img/txgraph.png)

## Dependencies

- graphviz, required for dot
- feh
