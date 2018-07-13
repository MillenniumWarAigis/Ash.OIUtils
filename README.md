[![Latest Release](https://img.shields.io/badge/version-1.4.0-brightgreen.svg)](../../../Ash.OIUtils/releases) [![Build Status](https://travis-ci.org/MillenniumWarAigis/Ash.OIUtils.svg?branch=master)](https://travis-ci.org/MillenniumWarAigis/Ash.OIUtils) ![Console App Output](https://img.shields.io/badge/output-console_app-green.svg) ![.NET Framework](https://img.shields.io/badge/%2ENET_framework-4%2E5%2E2-green.svg) ![C# Language](https://img.shields.io/badge/language-C%23-yellow.svg) [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

# Usage

Drop `.3`, `.6`, and `.7` archive files or directories onto the program window or icon and it'll extract the files within it to the designated output folder (by default: `out`).

The first file entry in most archives is not a `PNG` nor a `JPG` image. Its contents and file format is unknown. But if you still wish to extract it, use the option:

```console
/u=true
```

Most of the file entries are 1 by 1 pixel images. They're likely unused assets, but if you still wish to extract them, use the option:

```console
/s=true
```

You can inspect the extraction process/output with the option:

```console
/v=4
```

it will output something like this:

```console
    ->  80/82... PNG 1     1     8   6   0   0   0   (923 bytes)
```

- `<-` means the file was rejected with the current settings (*either because it isn't an image or only a 1x1 image*)
- `->` means the file was successfully written.
- The two numbers following the file format is the image width and height, respectively.
- You don't need to concern yourself with the other stuff!

# Known Issues

- Relative link operands, such as `./` and `../`, are likely not supported.
