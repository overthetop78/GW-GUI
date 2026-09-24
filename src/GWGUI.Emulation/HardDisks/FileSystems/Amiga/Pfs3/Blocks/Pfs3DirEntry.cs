namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    using System;

    public class Pfs3DirEntry : IPfs3DirEntry
    {
        // struct Pfs3DirEntry
        // {
        //     UBYTE next;             /* sizeof direntry                  */ 0
        //     BYTE  type;             /* dir, file, link etc              */ 1
        //     ULONG anode;            /* anode number                     */ 2
        //     ULONG fsize;            /* sizeof file                      */ 6
        //     UWORD creationday;      /* days since Jan. 1, 1978 (like ADOS; WORD instead of LONG) */ 10
        //     UWORD creationminute;   /* minutes past modnight            */ 12
        //     UWORD creationtick;     /* ticks past minute                */ 14
        //     UBYTE protection;       /* protection bits (like DOS)       */ 16
        //     UBYTE nlength;          /* lenght of filename               */ 17
        //     UBYTE startofname;      /* filename, followed by filenote length & filenote */ 18
        //     UBYTE pad;              /* make size even                   */ 19
        // };

        /// <summary>
        /// Size of dir entry
        /// </summary>
        public byte Next { get; private set; }

        /// <summary>
        /// Type of dir entry: dir, file, link etc
        /// </summary>
        public sbyte type { get; private set; }

        /// <summary>
        /// anode number
        /// </summary>
        public uint anode { get; private set; }

        /// <summary>
        /// sizeof file
        /// </summary>
        public uint fsize { get; private set; }

        /// <summary>
        /// Creation date of dir entry
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// protection bits (0 = RWED)
        /// </summary>
        public byte protection { get; private set; }

        public string Name { get; private set; }

        /// <summary>
        /// Start of name offset in dir entry struct
        /// </summary>
        public static readonly byte StartOfName = 17;

        public string comment { get; private set; }

        /// <summary>
        /// dir entry extra fields, null if dir entry doesn't have extra fields
        /// </summary>
        public Pfs3ExtraFields ExtraFields { get; private set; }

        public uint Size => fsize;

        /// <summary>
        /// Link path is the path of the linked file or directory. Link path is null if entry is not a link entry.
        /// </summary>
        public string LinkPath { get; private set; }

        public Pfs3DirEntry(byte next, sbyte type, uint anode, uint fsize, byte protection, DateTime date, string name,
            string comment, Pfs3ExtraFields extraFields)
        {
            this.type = type;
            this.anode = anode;
            this.fsize = fsize;
            this.protection = protection;
            this.CreationDate = date;
            Name = name;
            this.comment = comment;
            ExtraFields = extraFields;
            Next = next;
        }

        public Pfs3DirEntry(byte next, sbyte type, uint anode, uint fsize, byte protection, DateTime date, string name,
            string comment, Pfs3ExtraFields extraFields, Pfs3GlobalData g)
        {
            this.type = type;
            this.anode = anode;
            this.fsize = fsize;
            this.protection = protection;
            this.CreationDate = date;
            Name = name;
            this.comment = comment;
            ExtraFields = extraFields;
            Next = next == 0 ? (byte)CalculateSize(name, comment, extraFields, g) : next;
        }

        public Pfs3DirEntry(Pfs3DirEntry dirEntry, Pfs3GlobalData g) : this(dirEntry.Next, dirEntry.type, dirEntry.anode,
            dirEntry.fsize, dirEntry.protection, dirEntry.CreationDate, dirEntry.Name, dirEntry.comment,
            dirEntry.ExtraFields, g)
        {
        }

        public Pfs3DirEntry Clone() =>
            new Pfs3DirEntry(Next, type, anode, fsize, protection, CreationDate, Name, comment,
                new Pfs3ExtraFields(ExtraFields));

        public void SetType(sbyte type)
        {
            this.type = type;
        }

        public void SetAnode(uint anode)
        {
            this.anode = anode;
        }

        public void SetFSize(uint fSize)
        {
            this.fsize = fSize;
        }

        public void SetDate(DateTime date)
        {
            this.CreationDate = date;
        }

        public void SetProtection(byte prot)
        {
            this.protection = prot;
        }

        public Pfs3ExtraFields GetExtraFields() => new Pfs3ExtraFields(ExtraFields);

        public void SetExtraFields(Pfs3ExtraFields extraFields, Pfs3GlobalData g)
        {
            var updateSize = this.ExtraFields.ExtraFieldsSize != extraFields.ExtraFieldsSize;
            this.ExtraFields = extraFields;
            if (updateSize)
            {
                Next = (byte)CalculateSize(Name, comment, extraFields, g);
            }
        }

        public void SetLinkPath(string linkPath)
        {
            LinkPath = linkPath;
        }

        public Pfs3DirEntry() :
            this(0) // : this(0, 0, 0, 0, 0, DateTime.Now, string.Empty, string.Empty, new extrafields(), g)
        {
        }

        public Pfs3DirEntry(byte next)
        {
            this.type = type;
            this.anode = anode;
            this.fsize = fsize;
            this.protection = protection;
            this.CreationDate = DateTime.Now;
            Name = string.Empty;
            this.comment = string.Empty;
            ExtraFields = new Pfs3ExtraFields();
            Next = next;
        }

        private static int CalculateSize(string name, string comment, Pfs3ExtraFields extraFields, Pfs3GlobalData g)
        {
            // entrysize = ((sizeof(struct Pfs3DirEntry) + strlen(name)) & 0xfffe);
            var entrysize = (Pfs3SizeOf.Pfs3DirEntrySize.Struct + name.Length + comment.Length) & 0xfffe;
            if (g.dirextension)
            {
                entrysize += extraFields.ExtraFieldsSize;
            }

            return entrysize;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Pfs3DirEntry dirEntry))
            {
                return false;
            }
            return Next == dirEntry.Next && Name == dirEntry.Name;
        }

        public override int GetHashCode() => Next.GetHashCode() ^ Name.GetHashCode();
    }
}
