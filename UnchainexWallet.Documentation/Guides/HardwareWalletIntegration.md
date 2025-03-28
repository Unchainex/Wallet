# Hardware wallet integration into Unchainex 

## Introduction 

This is a technical document written for Hardware Wallet manufacturers. It describes the steps that need to be done before a hardware wallet can be supported by Unchainex. 
Unchainex does not directly support Hardware Wallets, but it uses [Bitcoin Core's Hardware Wallet Interface (HWI)](https://github.com/bitcoin-core/HWI) which is a command-line tool that unifies the commands for many devices. Unchainex includes the HWI binary and calls it every time whenever it interacts with the device. 

Unchainex's main priorities regarding hardware wallets:
- Compatibility - past, present, future
- Privacy
- Industry standardization

## Integration procedure

1. Integrate your hardware wallet into [HWI](https://github.com/bitcoin-core/HWI).
2. Add some tests into HWI, so compatibility issues can be caught at the early stages.
3. Test the compatibility with Unchainex. Import, Receive, Send, and Recover.
4. Write some [Kata tests](https://github.com/Unchainex/Wallet/blob/master/UnchainexWallet.Tests/AcceptanceTests/HwiKatas.cs) (manual tests), that can be used to detect compatibility issues when: Unchainex release, new firmware, etc.
5. Send at least one device (preferably two devices) to Unchainex HQ. To do so, contact us at `info@unchainex.org`.
6. Unchainex team tests the device using the Kata tests and approves.
7. Create some content on how to use the device with our software, possibly in the Unchainex documentation.
8. Create a guide on how to initialize the device in offline mode - if possible. There should be a method to init the device without sharing the xpub.
9. During an initial testing period of at least half a year, Unchainex will unofficially support the hardware wallet - meaning that it is working but nothing is guaranteed. 
10. After a half-year grace period without compatibility problems or breaking changes, Unchainex will officially support this hardware wallet, meaning that Unchainex will give guidance related to the device and announce the [support of the device](https://github.com/Unchainex/Wallet/blob/master/UnchainexWallet.Documentation/UnchainexCompatibility.md).
