# BitTorrent Client In C#

A compact BitTorrent client written in C# and .NET 8, originally built as part of the CodeCrafters "Build Your Own BitTorrent" challenge.

This is a learning-oriented networking project. It focuses on the mechanics behind torrent metadata, tracker communication, peer handshakes, piece requests, integrity checks, and magnet links rather than trying to be a polished replacement for a full BitTorrent client.

## Why This Project Exists

This project explores the lower-level pieces that a BitTorrent client needs before a user interface, persistence layer, or long-running session manager can be useful.

It was built to work directly with:

- bencode decoding and encoding
- `.torrent` metainfo parsing
- SHA-1 info hash and piece hash validation
- HTTP tracker announce requests
- compact peer list parsing
- BitTorrent peer handshakes
- interested, unchoke, bitfield, and piece messages
- block-level piece downloading
- concurrent piece downloading across discovered peers
- magnet URI parsing
- BitTorrent extension handshakes for `ut_metadata`

## What It Currently Supports

The CLI exposes the challenge stages as direct commands:

- `decode` decodes bencoded values
- `info` prints tracker, file, info hash, piece length, and piece hash metadata from a `.torrent` file
- `peers` asks the tracker for peers and prints compact peer addresses
- `handshake` performs a BitTorrent peer handshake
- `download_piece` downloads and verifies one piece from a torrent
- `download` downloads all pieces from a torrent and writes the assembled file
- `magnet_parse` parses a magnet URI
- `magnet_handshake` performs a peer handshake from a magnet URI
- `magnet_info` fetches torrent metadata through the extension protocol
- `magnet_download_piece` downloads one piece from a magnet URI
- `magnet_download` downloads all pieces from a magnet URI

## Design Notes

The code deliberately keeps the architecture direct:

- [src/Program.cs](src/Program.cs) maps CLI commands to command handlers
- [src/Bencoding](src/Bencoding) contains the bencode reader and writer
- [src/Metadata](src/Metadata) models torrent and magnet metadata
- [src/Connection](src/Connection) contains peer connection and protocol message types
- [src/Extensions/TrackerExtensions.cs](src/Extensions/TrackerExtensions.cs) handles tracker announce requests and compact peer parsing
- [src/Extensions/NetworkStreamExtensions.cs](src/Extensions/NetworkStreamExtensions.cs) handles peer protocol reads, writes, piece downloads, and piece hash verification

The implementation favors traceable protocol code over framework-style abstraction. For a project meant to demonstrate systems and protocol work, I kept the path from CLI input to network I/O easy to follow.

## Scope And Limitations

This is not a production BitTorrent client.

Important limitations:

- only single-file torrents are in scope
- tracker support is HTTP-oriented and minimal
- there is no DHT, PEX, peer choking strategy, resume data, or long-running session state
- peer selection and retry behavior are intentionally simple
- magnet metadata fetching currently targets the challenge-sized happy path rather than arbitrary large metadata responses
- diagnostics are written directly to the console instead of using structured logging
- there is no comprehensive automated test suite

The code is best read as a protocol implementation exercise, not as an application meant for everyday torrenting.

## Prerequisites

- Docker

You do not need a local .NET SDK for the Docker workflow below.

## Build The Image

Run this once from the repository root:

```console
$ docker build -t codecrafters-bittorrent-csharp .
```

## Run Commands In Docker

The container runs the BitTorrent CLI directly. Bind-mount the repository into `/workspace` so the container can read torrent files from your working tree.

```console
$ docker run --rm -v "$PWD:/workspace" codecrafters-bittorrent-csharp info sample.torrent
```

Ask the tracker for peers:

```console
$ docker run --rm -v "$PWD:/workspace" codecrafters-bittorrent-csharp peers sample.torrent
```

Parse a magnet URI:

```console
$ docker run --rm -v "$PWD:/workspace" codecrafters-bittorrent-csharp magnet_parse "magnet:?xt=urn:btih:d69f91e6b2ae4c542468d1073a71d4ea13879a7f&dn=sample&tr=http%3A%2F%2Fbittorrent-test-tracker.codecrafters.io%2Fannounce"
```

The magnet example uses the info hash and tracker from `sample.torrent`. The `peers` command contacts the CodeCrafters tracker, so it needs outbound network access from Docker.

## Local .NET Alternative

If you do have the .NET 8 SDK installed, the CodeCrafters helper script still works:

```console
$ ./your_bittorrent.sh info sample.torrent
$ ./your_bittorrent.sh peers sample.torrent
$ ./your_bittorrent.sh download -o tmp/output.bin sample.torrent
```

You can also run the project directly:

```console
$ dotnet run --project . --configuration Release -- info sample.torrent
```

## Authorship And AI Usage

The BitTorrent implementation in this repository was written by me as part of hands-on CodeCrafters practice.

AI assistance was used later for repository analysis and documentation work, including this README. The protocol implementation itself should be evaluated as a compact learning project rather than as AI-generated application scaffolding.

## What I Would Improve Next

If I continue developing this project, the next areas I would focus on are:

- support multi-file torrents
- make peer retry behavior bounded and easier to reason about
- fetch multi-piece magnet metadata correctly
- add focused tests around bencode parsing, tracker response parsing, and piece assembly
- introduce clearer command-line validation and help output
- replace console diagnostics with structured logging
