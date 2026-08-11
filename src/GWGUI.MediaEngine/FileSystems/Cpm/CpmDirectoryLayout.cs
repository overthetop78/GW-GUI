namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Décrit la position et l'allocation d'un répertoire CP/M à reconnaître.</summary>
/// <param name="DirectoryOffset">Position du premier octet du répertoire.</param>
/// <param name="AllocationOrigin">Origine des blocs d'allocation.</param>
/// <param name="DirectoryEntries">Nombre d'entrées du répertoire.</param>
/// <param name="AllocationBlockSize">Taille d'un bloc d'allocation.</param>
/// <param name="DirectoryBlocks">Nombre de blocs réservés au répertoire.</param>
/// <param name="WideAllocations">Indique si les numéros de blocs occupent deux octets.</param>
internal readonly record struct CpmDirectoryLayout(int DirectoryOffset, int AllocationOrigin, int DirectoryEntries, int AllocationBlockSize, int DirectoryBlocks, bool WideAllocations);
