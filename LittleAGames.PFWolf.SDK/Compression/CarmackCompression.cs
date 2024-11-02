﻿namespace LittleAGames.PFWolf.SDK.Compression;

public class CarmackCompression : ICompression<ushort>
{
    private const ushort NEAR_POINTER_FLAG = 0xA7;  // Flag for near pointers
    private const ushort FAR_POINTER_FLAG = 0xA8;   // Flag for far pointers
    
    public ushort[] Expand(byte[] source)
    {
        List<ushort> data = new List<ushort>();

        using (MemoryStream ms = new MemoryStream(source))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ushort value = reader.ReadUInt16();

                if (value == NEAR_POINTER_FLAG || value == FAR_POINTER_FLAG)
                {
                    ushort repeatValue = reader.ReadUInt16();
                    data.Add(repeatValue);
                    data.Add(repeatValue);
                }
                else
                {
                    data.Add(value);
                }
            }
        }
        return data.ToArray();
    }

    public byte[] Compress(ushort[] source)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            for (int i = 0; i < source.Length; i++)
            {
                ushort value = source[i];

                // Check for near and far pointer patterns
                if (value == NEAR_POINTER_FLAG || value == FAR_POINTER_FLAG)
                {
                    // Write a flag followed by the repeated data value
                    writer.Write(value);
                    writer.Write(value);
                }
                else
                {
                    // Write the data as-is
                    writer.Write(value);
                }
            }
            return ms.ToArray();
        }
    }
}