"""Leitura limitada a FBX binário 7400 para verificar os assets versionados."""

import struct
import zlib


def read_nodes(data, offset=27):
    result = []
    while offset + 13 <= len(data):
        end, count, length, size = struct.unpack_from("<IIIB", data, offset)
        if not end:
            break
        name = data[offset + 13 : offset + 13 + size].decode()
        pos = offset + 13 + size
        properties = []
        for _ in range(count):
            kind = chr(data[pos])
            pos += 1
            if kind in "SR":
                (n,) = struct.unpack_from("<I", data, pos)
                pos += 4
                value = data[pos : pos + n]
                pos += n
                properties.append(value.decode(errors="replace") if kind == "S" else value)
            elif kind in "YCLFID":
                fmt = {"Y": "h", "C": "?", "L": "q", "F": "f", "I": "i", "D": "d"}[kind]
                properties.append(struct.unpack_from("<" + fmt, data, pos)[0])
                pos += struct.calcsize(fmt)
            elif kind in "fdlibc":
                n, compressed, byte_count = struct.unpack_from("<III", data, pos)
                pos += 12
                raw = data[pos : pos + byte_count]
                pos += byte_count
                if compressed:
                    raw = zlib.decompress(raw)
                fmt = {"f": "f", "d": "d", "l": "q", "i": "i", "b": "?", "c": "b"}[kind]
                properties.append(struct.unpack("<" + str(n) + fmt, raw))
            else:
                raise ValueError("Propriedade FBX desconhecida: " + kind)
        stop = offset + 13 + size + length
        children = read_nodes(data, stop) if stop < end - 13 else []
        result.append((name, properties, children))
        offset = end
    return result
