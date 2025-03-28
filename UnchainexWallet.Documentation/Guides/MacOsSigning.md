# MacOS release environment guide (for signing and notarizating)

## How to create appleId and get code signing certificate

1. Get a macintosh machine. Apple developer program require two factor authentication.
2. Enroll in the apple developer program.
3. Create a New Certificate https://developer.apple.com/account/resources/certificates/list type: Developer ID Application (This certificate is used to code sign your app for distribution outside of the Mac App Store.)
4. Download the .cer file.
5. Use Keychain Access to export Personal Information Exchange (.p12) file. https://knowledge.digicert.com/solution/SO25463.html
6. Copy the .p12 file to desktop and run UnchainexWallet.Packager/Content/Osx/addcert.sh => Go to terminal ./addcert.sh - might run chmod u+x addcert.sh.
7. Get application specific password: https://support.apple.com/en-us/HT204397. Use label: "notarizate".
8. Now you can use set up the environment.

## Setting up the signing environment on macOS

1. Get a macintosh machine.
2. Create a new mac user caller Release.
3. Clone UnchainexWallet repository.
4. Get files from Unchainex safe storage: macdevsign.p12 and macsignpassw.txt. Copy them to desktop.
5. Run UnchainexWallet.Packager/Content/Osx/addcert.sh => Go to terminal ./addcert.sh - might run chmod a+rx addcert.sh.
6. Install XCode*

## Procedure in a few words

1. Publish with dotnet to osx.
2. Create the App folder, specify plist file.
3. Sign with entitlements.
4. Notarize.
4. Staple.
5. Create the dmg file.
6. Sign with entitlements.
7. Notarize.
8. Staple.
9. Dmg ready to distribute.

## Source

- https://github.com/btcpayserver/BTCPayServer.Vault/blob/master/Build/travis/applesign.sh
- https://developer.apple.com/library/archive/technotes/tn2206/_index.html
- https://developer.apple.com/developer-id/

(The altool and stapler command-line tools (included within Xcode) allow you to upload your software to the Apple notary service, and to staple the resulting ticket to your executable. altool is located at /Applications/Xcode.app/Contents/Developer/usr/bin/altool.). Unfortunately it is not enough to install Command Line Tool for XCode. 

## Changing the icon

- Wild specificaiton for the icon:
  - 1024x1024 size
  - 98px padding
  - BoxShadow params: x:0, y:5, Blur:25, Spread:0, Color:000000, Transparency:50%

## Changing the background of dmg installer

1. Get a Mac
2. Install create-dmg https://github.com/andreyvit/create-dmg 
3. Create a folder called somewhere called unchainexdmg
4. Get `Unchainex Wallet.app` for example you can download the latest release and extract it from dmg(7zip). Then copy it under unchainexdmg.
5. Copy the following two files under unchainexdmg Logo_with_text_small.png and UnchainexLogo.icns
6. Open terminal at unchainexdmg
7. Set the version number in the following command and run it
9. `create-dmg --volname "Unchainex Wallet" --volicon "UnchainexLogo.icns" --background "Logo_with_text_small.png" --window-pos 200 120 --window-size 600 440 --icon "Unchainex Wallet.app" 110 150 --app-drop-link 500 150 --hdiutil-verbose "Unchainex.dmg" "Unchainex Wallet.app/"`
11. Dmg file is created under unchainexdmg
12. Copy the dmg to windows computer, extract(7zip) DS_Store file and copy it to `UnchainexWallet\UnchainexWallet.Packager\Content\Osx\Dmg\DS_Store.dat` - add the extension .dat to prevent macOs overwriting the file.
13. Copy the new background PNG to `UnchainexWallet\UnchainexWallet.Packager\Content\Osx\Dmg\.background`
14. Now you can run the packager procedure
