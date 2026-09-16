namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using Blocks;
    using Core.Converters;

    public static class Pfs3DirEntryWriter
    {
        public static void Write(byte[] data, int offset, int next, Pfs3DirEntry dirEntry, Pfs3GlobalData g)
        {
            data[offset] = (byte)next;
            data[offset + 1] = (byte)dirEntry.type;
            BigEndianConverter.ConvertUInt32ToBytes(dirEntry.anode, data, offset + 2);
            BigEndianConverter.ConvertUInt32ToBytes(dirEntry.fsize, data, offset + 6);
            Pfs3DateHelper.WriteDate(dirEntry.CreationDate, data, offset + 10);
            data[offset + 16] = dirEntry.protection;
            data[offset + 17] = (byte)dirEntry.Name.Length;
            var nameBytes = AmigaTextHelper.GetBytes(dirEntry.Name);
            Array.Copy(nameBytes, 0, data, offset + Pfs3DirEntry.StartOfName + 1, nameBytes.Length);

            var commentBytes = AmigaTextHelper.GetBytes(dirEntry.comment ?? string.Empty);
            var commentOffset = offset + Pfs3DirEntry.StartOfName + 1 + dirEntry.Name.Length;
            data[commentOffset] = (byte)dirEntry.comment.Length;
            if (!string.IsNullOrEmpty(dirEntry.comment))
            {
                Array.Copy(commentBytes, 0, data, commentOffset + 1, commentBytes.Length);
            }

            // set last dir entry byte to 0, if size is odd (used together with entry size calculation binary and 0xfffe)
            if (((commentOffset + 1 + commentBytes.Length) & 1) == 1)
            {
                data[commentOffset + 1 + commentBytes.Length] = 0;
            }

            if (g.dirextension)
            {
                WriteExtraFields(data, offset, dirEntry);
            }
        }

        private static void WriteExtraFields(byte[] data, int offset, Pfs3DirEntry dirEntry)
        {
            // UWORD offset, *dirext;
            // UWORD array[16], i = 0, j = 0;
            // UWORD flags = 0, orvalue;
            // UWORD *fields = (UWORD *)extra;

            ushort flags = 0;
            var fieldsIndex = 0;
            var fieldsArray = Pfs3ExtraFields.ConvertToUShortArray(dirEntry.ExtraFields);
            var array = new List<ushort>();

            // offset = (sizeof(struct Pfs3DirEntry) + (direntry->nlength) + *COMMENT(direntry)) & 0xfffe;
            // dirext = (UWORD *)((UBYTE *)(direntry) + (UBYTE)offset);
            var extraFieldsOffset = (Pfs3SizeOf.Pfs3DirEntrySize.Struct + dirEntry.Name.Length + dirEntry.comment.Length) & 0xfffe;
            var dirext = extraFieldsOffset;

            ushort orvalue = 1;
            // /* fill packed field array */
            int i;
            for (i = 0; i < Pfs3SizeOf.Pfs3ExtraFieldsSize.Struct / 2; i++)
            {
                if (fieldsArray[fieldsIndex] != 0)
                {
                    //array[j++] = *fields++;
                    array.Add(fieldsArray[fieldsIndex]);
                    fieldsIndex++;
                    flags |= orvalue;
                }
                else
                {
                    fieldsIndex++;
                }

                orvalue <<= 1;
            }

            // /* add fields to direntry */
            i = array.Count;
            while (i > 0)
            {
                //     *dirext++ = array[--i];
                BigEndianConverter.ConvertUInt16ToBytes(array[--i], data, offset + dirext);
                dirext += 2; // 2 bytes increase for ushort/uword
            }
            // *dirext++ = flags;
            BigEndianConverter.ConvertUInt16ToBytes(flags, data, offset + dirext);

            // direntry.next = offset + 2 * j + 2;
            //dirEntry.next = (byte)(extraFieldsOffset + 2 * j + 2);
        }
    }
}
