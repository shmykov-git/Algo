namespace Font.Model;

public enum FontType
{
    uint8,  //8-bit unsigned integer.
    int8,   //8-bit signed integer.
    uint16, //16-bit unsigned integer.
    int16,  //16-bit signed integer.
    uint24, //24-bit unsigned integer.
    uint32, //32-bit unsigned integer.
    int32,  //32-bit signed integer.
    uint64,
    int64,
    Fixed,  //32-bit signed fixed-point number (16.16)
    FWORD,  //int16 that describes a quantity in font design units.
    UFWORD, //uint16 that describes a quantity in font design units.
    F2DOT14,    //16-bit signed fixed number with the low 14 bits of fraction (2.14).
    LONGDATETIME,   //Date and time represented in number of seconds since 12:00 midnight, January 1, 1904, UTC. The value is represented as a signed 64-bit integer.
    Tag,    //Array of four uint8s (length = 32 bits) used to identify a table, design-variation axis, script, language system, feature, or baseline
    Offset16,   //Short offset to a table, same as uint16, NULL offset = 0x0000
    Offset24,   //24-bit offset to a table, same as uint24, NULL offset = 0x000000
    Offset32,   //Long offset to a table, same as uint32, NULL offset = 0x00000000
    Version16Dot16,	//Packed 32-bit value with major and minor version numbers. (See Table Version Numbers.)
    Array,    // byte array with length parameter
    String,   // byte array with length parameter known as string
    OffsetString,   // byte array with length parameter known as string located at offset
    ArrayX2,  // double byte array with length parameter
    ArrayGlyph,  // byte array of glyphs
}
