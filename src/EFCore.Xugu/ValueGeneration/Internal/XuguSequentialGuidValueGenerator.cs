using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;

/// <summary>
///     Generates sequential GUIDs for clustered index performance. Client-side generation;
///     XuguDB provides <c>SYS_GUID()</c> for server-side defaults (see guid.md).
/// </summary>
public class XuguSequentialGuidValueGenerator : ValueGenerator<Guid>
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public override Guid Next(EntityEntry entry)
        => Next();

    public virtual Guid Next()
        => Next(DateTimeOffset.UtcNow);

    public virtual Guid Next(DateTimeOffset timeNow)
    {
        var randomBytes = new byte[7];
        Rng.GetBytes(randomBytes);
        var ticks = (ulong)timeNow.Ticks;

        const ushort uuidVersion = 4;
        const ushort uuidVariant = 0b1000;

        var ticksAndVersion = (ushort)((ticks << 48 >> 52) | (ushort)(uuidVersion << 12));
        var ticksAndVariant = (byte)((ticks << 60 >> 60) | (byte)(uuidVariant << 4));

        return new Guid(
            (uint)(ticks >> 32),
            (ushort)(ticks << 32 >> 48),
            ticksAndVersion,
            ticksAndVariant,
            randomBytes[0],
            randomBytes[1],
            randomBytes[2],
            randomBytes[3],
            randomBytes[4],
            randomBytes[5],
            randomBytes[6]);
    }

    public override bool GeneratesTemporaryValues => false;
}
