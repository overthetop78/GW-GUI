namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public static partial class Pfs3Directory
    {
        public static async Task<bool> RenameAndMove(Pfs3ObjectInfo sourcedi, Pfs3ObjectInfo srcinfo, Pfs3ObjectInfo destdi,
            string destname, Pfs3GlobalData g)
        {
            Pfs3DirEntry srcdirentry, destentry;
            // var entrybuffer = new byte[Macro.MAX_ENTRYSIZE];
            uint srcanodenr, destanodenr;
            short srcfieldoffset, destfieldoffset, fieldsize;
            Pfs3ObjectInfo destinfo = new Pfs3ObjectInfo();
            //objectinfo destdi = new objectinfo();
            //string srccomment, destcomment;
            //string destname;

            // COMMENTED: Already set
            // /* fetch source info & path and check if exists */
            // if (!(destname = GetFullPath (destdir, destpath, destdi, error, g)))
            // {
            //     throw new IOException("ERROR_OBJECT_NOT_FOUND");
            // }

            /* source nor destination may be a volume */
            if (Pfs3Macro.IsVolume(srcinfo) || string.IsNullOrEmpty(destname))
            {
                throw new IOException("ERROR_OBJECT_WRONG_TYPE");
            }

            // #if DELDIR
            if (Pfs3Macro.IsDelDir(sourcedi) || Pfs3Macro.IsDelDir(destdi) || Pfs3Macro.IsDelDir(srcinfo))
            {
                throw new IOException("ERROR_WRITE_PROTECTED");
            }
            // #endif

            /* check reserved area lock */
            if (Pfs3Macro.ReservedAreaIsLocked(g))
            {
                throw new IOException("ERROR_DISK_FULL");
            }

            srcdirentry = srcinfo.file.direntry;
            //srccomment = Macro.COMMENT(srcdirentry);

            /* check if new name allowed
             * destpath should exist and file should not
             * %9.1 the same name IS allowed (rename 'hello' to 'Hello')
             */
            destinfo = destdi.Clone();
            if (!(await Find(destinfo, destname, g)).Any())
            {
                if (!destinfo.file.direntry.Equals(srcinfo.file.direntry))
                {
                    throw new IOException("ERROR_OBJECT_EXISTS");
                }
            }

            /* Test if a directory is being renamed to a child of itself. This is so
             * if:
             * 1) source (srcinfo) is a directory and
             * 2) sourcepath (sourcedi) <> destinationpath (destdi) and
             * 3) source (srcinfo) is part of destpath (destdi)
             * Example: rename a/b to a/b/c/d:
             * 1) a/b is dir [ok]; 2) a <> a/b/c [ok]; 3) a/b is part of a/b/c [ok]
             * Links need special attention!
             */
            srcanodenr = Pfs3Macro.IsRootA(sourcedi) ? Pfs3Constants.ANODE_ROOTDIR : Pfs3Macro.FIANODENR(sourcedi.file);
            destanodenr = Pfs3Macro.IsRootA(destdi) ? Pfs3Constants.ANODE_ROOTDIR : Pfs3Macro.FIANODENR(destdi.file);
            if (Pfs3Macro.IsRealDir(srcinfo) && (srcanodenr != destanodenr) && await IsChildOf(destdi, srcinfo, g))
            {
                throw new IOException("ERROR_OBJECT_IN_USE");
            }

            /* Make destination  */
            //destentry = (struct Pfs3DirEntry *)&entrybuffer;
            //destentry = srcdirentry;
            destentry = new Pfs3DirEntry(0, srcdirentry.type, srcdirentry.anode, srcdirentry.fsize, srcdirentry.protection,
                srcdirentry.CreationDate, destname, srcdirentry.comment, srcdirentry.ExtraFields, g);

            /* copy header */
            //memcpy(destentry, srcdirentry, offsetof(struct Pfs3DirEntry, nlength));

            /* copy name */
            // destentry.nlength = (byte)destname.Length;
            //    if (destentry.nlength > Macro.FILENAMESIZE(g) - 1)
            //    {
            //        destentry.nlength = Macro.FILENAMESIZE(g) - 1;
            //    }
            // memcpy((UBYTE *)&destentry->startofname, destname, destentry->nlength);
            //
            // /* copy comment */
            // destcomment = (UBYTE *)&destentry->startofname + destentry->nlength;
            // memcpy(destcomment, srccomment, *srccomment + 1);

            /* copy fields */
            // srcfieldoffset = (short)((Pfs3SizeOf.Pfs3DirEntrySize.Struct + srcdirentry.Name.Length + srcdirentry.comment.Length) & 0xfffe);
            // destfieldoffset = (short)((Pfs3SizeOf.Pfs3DirEntrySize.Struct + destname.Length + srcdirentry.comment.Length) & 0xfffe);
            // fieldsize = (byte)(srcdirentry.next - srcfieldoffset);
            //
            // if (g.dirextension)
            // {
            //     destentry.ExtraFields = srcdirentry.ExtraFields;
            //     //memcpy((UBYTE *)destentry + destfieldoffset, (UBYTE *)srcdirentry + srcfieldoffset, fieldsize);
            // }
            //
            // /* set size */
            // if (g.dirextension)
            //     destentry.next = (byte)(destfieldoffset + fieldsize);
            // else
            //     destentry.next = (byte)destfieldoffset;

            /* remove source and add new direntry
             * Makes srcinfo INVALID
             */
            //PFSDoNotify(&srcinfo->file, TRUE, g);

            await ChangeDirEntry(srcinfo, destentry, destdi, destinfo.file, g); // output:destinfo

            /* Pfs3Update linklist and notify source if object moved across dirs
             */
            if (destanodenr != srcanodenr)
            {
                await MoveLink(destentry, destanodenr, g);
                //PFSDoNotify (&destinfo.file, TRUE, g);
            }
            // else
            // {
            // 	//PFSDoNotify (&destinfo.file, FALSE, g);
            // }

            /* If object is a directory and parent changed, update dirblocks */
            if (Pfs3Macro.IsDir(destinfo) && (srcanodenr != destanodenr))
            {
                Pfs3Canode anode = new Pfs3Canode();
                uint anodeoffset;
                var gadoor = true;

                anode.nr = destinfo.file.direntry.anode;
                anodeoffset = 0;
                await Pfs3Anodes.GetAnode(anode, anode.nr, g);
                for (anodeoffset = 0; gadoor;)
                {
                    Pfs3CachedBlock blk; // cdirblock

                    blk = await LoadDirBlock(anode.blocknr + anodeoffset, g);
                    if (blk != null)
                    {
                        var dirBlockBlk = blk.dirblock;
                        dirBlockBlk.parent = destanodenr; // destination dir
                        await Pfs3Update.MakeBlockDirty(blk, g);
                    }

                    var nextBlockResult = await Pfs3Anodes.NextBlock(anode, anodeoffset, g);
                    gadoor = nextBlockResult.Item1;
                    anodeoffset = nextBlockResult.Item2;
                }
            }

            return true;
        }

        public static async Task<bool> IsChildOf(Pfs3ObjectInfo child, Pfs3ObjectInfo parent, Pfs3GlobalData g)
        {
            Pfs3ObjectInfo up = new Pfs3ObjectInfo();
            bool goon = true;

            while (goon && !Pfs3Macro.IsSameOI(child, parent))
            {
                goon = await GetParent(child, up, g);
                child = up;
            }

            return Pfs3Macro.IsSameOI(child, parent);
        }

/*
 * Pfs3Update linklist to reflect moved node
 * (is supercopy of UpdateLinkDir)
 */
        public static async Task MoveLink(Pfs3DirEntry object_, uint newdiran, Pfs3GlobalData g)
        {
            Pfs3Canode linklist = new Pfs3Canode();
            uint linknr;

            //ENTER("MoveLink");
            // extrafields = GetExtraFields(entries, object_);
            var extrafields = object_.GetExtraFields();

            /* check if is link or linked to */
            if ((linknr = extrafields.link) == 0)
            {
                return;
            }

            /* check filetype */
            if (object_.type == Pfs3Constants.ST_LINKDIR || object_.type == Pfs3Constants.ST_LINKFILE)
            {
                /* it is a link -> just change the linkdir */
                await Pfs3Anodes.GetAnode(linklist, object_.anode, g);
                linklist.blocknr = newdiran;
                await Pfs3Anodes.SaveAnode(linklist, linklist.nr, g);
            }
            else
            {
                /* it is the head (linked to) */
                while (linknr != 0)
                {
                    /* update linklist: change clustersize (== object dir) */
                    await Pfs3Anodes.GetAnode(linklist, linknr, g);
                    linklist.clustersize = newdiran; /* the object's directory */
                    await Pfs3Anodes.SaveAnode(linklist, linklist.nr, g);
                    linknr = linklist.next;
                }
            }
        }

/* AddComment
 *
 * - get old direntry
 * - make new direntry
 * - remove old direntry
 * - add new direntry
 *
 * maxdirty: 1d, 1a = 2 res
 */
        public static async Task<bool> AddComment(Pfs3ObjectInfo info, string comment, Pfs3GlobalData g)
        {
            Pfs3DirEntry sourceentry, destentry;
            Pfs3ObjectInfo directory = new Pfs3ObjectInfo();
            //UBYTE *destcomment, *srccomment, entrybuffer[MAX_ENTRYSIZE];
            short srcfieldoffset, destfieldoffset, fieldsize;

//            DB(Trace(1, "AddComment", "%s\n", comment));
            // #if DELDIR
	        if (info.deldir.special <= Pfs3Constants.SPECIAL_DELFILE)
	        {
		        throw new IOException("ERROR_WRITE_PROTECTED");
	        }
            // #endif

            if (comment.Length > Pfs3Constants.CMSIZE)
            {
                throw new IOException("ERROR_COMMENT_TOO_BIG");
            }

            /* check reserved area lock */
            if (Pfs3Macro.ReservedAreaIsLocked(g))
            {
                throw new IOException("ERROR_DISK_FULL");
            }

            /* make new direntry */
            // destentry = (struct Pfs3DirEntry *)entrybuffer;
            sourceentry = info.file.direntry;

            destentry = new Pfs3DirEntry(0, sourceentry.type, sourceentry.anode, sourceentry.fsize, sourceentry.protection,
                sourceentry.CreationDate, sourceentry.Name, comment, sourceentry.ExtraFields, g);

            // destentry = new direntry(sourceentry)
            // {
            //     type = sourceentry.type,
            //     anode = sourceentry.anode,
            //     fsize = sourceentry.fsize,
            //     protection = sourceentry.protection,
            //     CreationDate = sourceentry.CreationDate,
            //     Name = sourceentry.Name,
            //     comment = comment
            // };

            /* copy header & name */
            // memcpy(destentry, sourceentry, sizeof(struct Pfs3DirEntry) + sourceentry->nlength - 1);

            /* copy comment */
            // destcomment = COMMENT(destentry);
            // *destcomment = strlen(comment);
            // memcpy(destcomment + 1, comment, *destcomment);

            /* copy fields */
            //srccomment = COMMENT(sourceentry);
            // srcfieldoffset = (short)((Pfs3SizeOf.Pfs3DirEntrySize.Struct + sourceentry.Name.Length + sourceentry.comment.Length) & 0xfffe);
            // destfieldoffset = (short)((Pfs3SizeOf.Pfs3DirEntrySize.Struct + sourceentry.Name.Length + comment.Length) & 0xfffe);
            // fieldsize = (short)(sourceentry.next - srcfieldoffset);
            //
            // /* set size */
            // if (g.dirextension)
            //     destentry.next = (byte)(destfieldoffset + fieldsize);
            // else
            //     destentry.next = (byte)destfieldoffset;
            //
            // if (g.dirextension)
            // {
            //     // memcpy((UBYTE *)destentry + destfieldoffset, (UBYTE *)sourceentry + srcfieldoffset, fieldsize);
            //     destentry.ExtraFields = sourceentry.ExtraFields;
            // }

            /* remove old directoryentry and add new */
            if (!await GetParent(info, directory, g))
                return false;
            else
            {
                var fileInfo = new Pfs3FileInfo();
                await ChangeDirEntry(info, destentry, directory, fileInfo, g);
                info.file = fileInfo;
                return true;
            }
        }

        /*
         * linkdir: directory (in)
         * linkname: name (in)
         * object: object to link to (in) (dir must be locked)
         * newlink: result (out)
         */
        /// <summary>
        /// Create a link to an entry.
        /// </summary>
        /// <param name="linkdir">Pfs3Directory to create the link in.</param>
        /// <param name="linkname">Name of the link to create.</param>
        /// <param name="obj">Object to link to.</param>
        /// <param name="newlink">Newlink created.</param>
        /// <param name="g">Globaldata</param>
        /// <exception cref="IOException">Exception thrown.</exception>
        public static async Task CreateLink(Pfs3ObjectInfo linkdir, string linkname, Pfs3ObjectInfo obj,
				        Pfs3ObjectInfo newlink, Pfs3GlobalData g)
        {
	        var info = new Pfs3ObjectInfo();
            var odi = new Pfs3ObjectInfo();
	        uint anodenr, linklist;
            Pfs3DirEntry objectentry;
            Pfs3DirEntry destentry;
	        //byte[] entrybuffer = new byte[MAX_ENTRYSIZE];
	        Pfs3ExtraFields extrafields;
            Pfs3Canode linknode = new Pfs3Canode();
	        int l;

	        //ENTER("CreateLink");
            // #if DELDIR
            if (Pfs3Macro.IsDelDir(linkdir) || Pfs3Macro.IsDelDir(obj) || Pfs3Macro.IsDelFile(obj))
            {
                throw new IOException("ERROR_WRITE_PROTECTED");
            }
            // #endif

	        /* check if operation possible */
	        if (!g.dirextension)
	        {
                throw new IOException("ERROR_ACTION_NOT_KNOWN");
	        }

	        /* check disk-writeprotection etc */
            Pfs3VolumeOperations.CheckVolume(g.currentvolume, true, g);

	        /* get anodenr */
	        if (!Pfs3Macro.IsDirEntry(linkdir) || Pfs3Macro.IsVolume(linkdir))
	        {
		        anodenr = (uint)Pfs3Macro.ANODE_ROOTDIR;
	        }
	        else
	        {
		        anodenr = Pfs3Macro.FIANODENR(linkdir.file);
		        Pfs3Cache.LOCK(linkdir.file.dirblock, g);
	        }

            /* truncate filename to 31 characters */
            if ((l = linkname.Length) == 0)
            {
                throw new IOException("ERROR_INVALID_COMPONENT_NAME");
            }

            var fileNameSize = Pfs3Macro.FILENAMESIZE(g);
            if (l > fileNameSize - 1)
            {
                linkname = linkname.Substring(fileNameSize - 1);
            }

	        /* check reserved area lock */
	        if (Pfs3Macro.ReservedAreaIsLocked(g))
	        {
                throw new IOException("ERROR_DISK_FULL");
	        }

	        /* check if a file by that name already exists */
	        if (await SearchInDir(anodenr, linkname, info, g))
	        {
                throw new IOException("ERROR_OBJECT_EXISTS");
	        }

	        /* make directory entry
	         * the anode allocated is the link list element
	         */
            destentry = await MakeDirEntry(Pfs3Constants.ST_LINKDIR, linkname, g);
            if (destentry == null)
            {
                throw new IOException("Failed to make dir entry");
            }

	        /* add link info */
	        //destentry = (struct Pfs3DirEntry *)entrybuffer;
	        objectentry = obj.file.direntry;
        // #if MULTIUSER
	       //  //GetExtraFields(destentry, &extrafields);
        //     extrafields = new extrafields(destentry.ExtraFields);
        // #else
            // memset(&extrafields, 0, sizeof(struct Pfs3ExtraFields));
            extrafields = new Pfs3ExtraFields();
        // #endif
	        extrafields.SetLink(objectentry.anode);
	        //AddExtraFields(destentry, extrafields);
            destentry.SetExtraFields(extrafields, g);

	        /* copy object info */
	        destentry.SetFSize(objectentry.fsize);
	        switch (objectentry.type)
	        {
		        case Pfs3Constants.ST_FILE:
		        case Pfs3Constants.ST_SOFTLINK:
		        case Pfs3Constants.ST_ROLLOVERFILE:
		        case Pfs3Constants.ST_LINKFILE:
			        destentry.SetType(Pfs3Constants.ST_LINKFILE);
			        break;
		        default:
			        destentry.SetType(Pfs3Constants.ST_LINKDIR);
			        break;
	        }

	        /* store directoryentry */
	        if (!await AddDirectoryEntry(linkdir, destentry, newlink.file, g))
	        {
                //return DOSFALSE;
		        await Pfs3Anodes.FreeAnode(destentry.anode, g);
                return;
            }

	        /* make linknode */
	        linknode.clustersize = obj.file.dirblock.dirblock.anodenr;
	        linknode.blocknr = newlink.file.dirblock.dirblock.anodenr;
	        linknode.next = 0;
	        await Pfs3Anodes.SaveAnode(linknode, newlink.file.direntry.anode, g);

	        /* change objectentry */
	        //GetExtraFields(objectentry, &extrafields);
            extrafields = objectentry.GetExtraFields();
	        if (extrafields.link == 0)
	        {
		        /* there were no links yet ->
		         * add link field. Use entrybuffer and destentry pointer
		         */
		        extrafields.SetLink(newlink.file.direntry.anode);
		        //memcpy(entrybuffer, objectentry, objectentry->next);
                destentry = new Pfs3DirEntry(objectentry, g);
		        //AddExtraFields(destentry, &extrafields);
                destentry.SetExtraFields(extrafields, g);

                if (!await GetParent(obj, odi, g))
                {
                    //return DOSFALSE;    /* serious! should not happen */
                    throw new IOException("Failed to get parent");
                }

		        await ChangeDirEntry(obj, destentry, odi, obj.file, g);
	        }
	        else
	        {
		        /* add new link to chain */
		        await Pfs3Anodes.GetAnode(linknode, extrafields.link, g);
                while (linknode.next != 0)
                {
                    await Pfs3Anodes.GetAnode(linknode, linknode.next, g);
                }
		        linknode.next = newlink.file.direntry.anode;
		        await Pfs3Anodes.SaveAnode(linknode, linknode.nr, g);
	        }

	        //return DOSTRUE;
        }

/* ProtectFile, SetDate
 *
 * - simple direntry in cache change, no change in size
 * - CACHEDDIRENTRY must have changeflag
 *
 * maxneeds: changes 1 block. NEVER allocates new block
 */
        public static async Task<bool> ProtectFile(Pfs3ObjectInfo file, uint protection, Pfs3GlobalData g)
        {
            //ENTER("ProtectFile");

            // isvolume check already done in dostohandler..
            //
            //  if (!file || !file->direntry)   /* @XLV */
            //  {
            //      *error = ERROR_OBJECT_WRONG_TYPE;
            //      return DOSFALSE;
            //  }

            // #if DELDIR
	        if (file.delfile.special <= Pfs3Constants.SPECIAL_DELFILE)
	        {
		        if (file.delfile.special == Pfs3Constants.SPECIAL_DELDIR)
		        {
			        protection &= Pfs3Constants.DELENTRY_PROT_AND_MASK;
			        protection |= Pfs3Constants.DELENTRY_PROT_OR_MASK;
			        g.currentvolume.rblkextension.rblkextension.dd_protection = protection;
			        await Pfs3Update.MakeBlockDirty(g.currentvolume.rblkextension, g);
			        return true;
		        }

		        throw new IOException("ERROR_WRITE_PROTECTED");
	        }
            // #endif

            /* check reserved area lock */
            if (Pfs3Macro.ReservedAreaIsLocked(g))
            {
                throw new IOException("ERROR_DISK_FULL");
            }

            file.file.direntry.SetProtection((byte)protection);

            /* add second part of protection */
            if (g.dirextension)
            {
                Pfs3ObjectInfo directory = new Pfs3ObjectInfo();
                // direntry sourceentry;
                // extrafields extrafields = new extrafields();
                // UBYTE entrybuffer[MAX_ENTRYSIZE];

                /* make new direntry */
                //destentry = (struct Pfs3DirEntry *)entrybuffer;
                //sourceentry = new direntry(file.file.direntry, g);

                // destentry = new direntry
                // {
                //     Offset = sourceentry.Offset,
                //     next = sourceentry.next,
                //     type = sourceentry.type,
                //     anode = sourceentry.anode,
                //     fsize = sourceentry.fsize,
                //     protection = (byte)protection,
                //     CreationDate = sourceentry.CreationDate,
                //     Name = sourceentry.Name,
                //     comment = sourceentry.comment
                // };

                /* copy source */
                //memcpy(destentry, sourceentry, sourceentry->next);

                /* set new extrafields */
                //var dirBlock = file.file.dirblock.dirblock;
                // extrafields = GetExtraFields(dirBlock.entries, sourceentry);
                var extraFields = file.file.direntry.GetExtraFields();
                extraFields.SetProtection(protection);

                // AddExtraFields(dirBlock.entries, destentry, extrafields);
                var destEntry = new Pfs3DirEntry(file.file.direntry, g);
                destEntry.SetExtraFields(extraFields, g);

                /* commit changes */
                if (!await GetParent(file, directory, g))
                {
                    return false;
                }
                else
                {
                    await ChangeDirEntry(file, destEntry, directory, file.file, g);
                }
            }

            /* mark block for update and return success */
            await Pfs3Update.MakeBlockDirty(file.file.dirblock, g);
            return true;
        }
    }
}
