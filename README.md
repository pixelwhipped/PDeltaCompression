# PDeltaCompression
Experimental Compression Concept

Proof of cencept, for a compression algorithm.
The method will only yeild good results in very limited circumstances.

# How it works.
Taking 9 bytes PDelta will order the bytes in order to reduce the differnce between the bytes.
The indices of the ordered 9 bytes are stored in 4 bytes esentially recording the permutation
and then the delta of the new byte sequences are added.

The savings are due to the bits required to represent the Delta
//1 Bit = (9*8)-((9)+(4*8) == 72 - 41 = 31 Bits saved 28 bits
//2 Bit = (9*8)-((2*9)+(4*8) == 72 - 50 = 22 Bits saved 19 bits
//3 Bit = (9*8)-((3*9)+(4*8) == 72 - 59 = 13 Bits saved 10 bits
//4 Bit = (9*8)-((4*9)+(4*8) == 72 - 68 = 4 Bits saved 1 bits