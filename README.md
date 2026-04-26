# TIPatcher
"Patches" TI Nspire CX CAS Student Software 

## Prerequisites
- A computer running Windows or Linux
- Administrator/superuser rights

## How to use

### Windows
1) Download and install the [TI Nspire CX CAS Student Software](https://education.ti.com/en/software/details/en/36BE84F974E940C78502AA47492887AB/ti-nspirecxcas_pc_full) for Windows
2) Download the latest release (patcher-windows.zip) from [Releases](https://github.com/ocelot910/TIPatcher/releases)
3) Extract patcher.zip and run TIPatcher.exe. When it asks to copy the patched file, input 'y'
4) Done! As long as you don't update the TI Nspire CX CAS Student Software, it will remain activated

### Linux
*Tested on Ubuntu 24.02 WSL and VM* 
1) Download and install [Wine](https://www.winehq.org/)
2) Download the latest release (patcher-linux.zip) from [Releases](https://github.com/ocelot910/TIPatcher/releases)
3) Extract patcher.zip and open the extracted directory in the Terminal
4) Run `chmod +x TIPatcher` and then `./TIPatcher`. When it asks to copy the patched file, input 'y'
5) Done! As long as you don't update the TI Nspire CX CAS Student Software, it will remain activated  
**Note**: If the font is missing (renders as boxes), then install Microsoft Core Fonts:
`sudo apt install ttf-mscorefonts-installer`

## Credits
[JavaResolver](https://github.com/Washi1337/JavaResolver) - Library used for patching Java instructions, which allows the patcher to work without a Java installation
