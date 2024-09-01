using Xunit;
using Xunit.Abstractions;

using System.Xml.Schema;
using NuGet.Frameworks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Runtime.CompilerServices;

namespace Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var akteur = new Minformat.Akteur();
        byte[] id1 = { 1, 0, 0, 0, 0, 0, 0, 0 };
        byte[] data1 = {9,8,7,6,5,4,3,1,0};

        byte[] id2 = { 255, 0, 0, 0, 0, 0, 0, 0 };
        byte[] data2 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        Dictionary<byte[], byte[]> bloecke = new Dictionary<byte[], byte[]>() { { id1, data1 }, { id2, data2 } };
        var r = akteur.NachrichtBauen(0, bloecke);

        (var nachr_id, var r_) = Minformat.Akteur.NachrichtZerlegen(r);

        Assert.True(r_.Where(e => id1.SequenceEqual(e.Key)).Count() == 1);
        Assert.True(r_.Where(e => id2.SequenceEqual(e.Key)).Count() == 1);
        Assert.True(r_.Where(e => data1.SequenceEqual(e.Value)).Count() == 1);
        Assert.True(r_.Where(e => data2.SequenceEqual(e.Value)).Count() == 1);

        var item = r_.ElementAt(0);
        Assert.True(item.Key.SequenceEqual(id1));
        Assert.True(item.Value.SequenceEqual(data1));

        item = r_.ElementAt(1);
        Assert.True(item.Key.SequenceEqual(id2));
        Assert.True(item.Value.SequenceEqual(data2));
    }

    [Fact]
    public void Test2()
    {
        var akteur = new Minformat.Akteur(7,7);
        byte[] id1 = { 1, 0, 0, 0, 4, 5, 6 };
        byte[] data1 = { 9, 8, 7, 6, 5, 4, 3, 1, 0 };

        byte[] id2 = { 255, 0, 0, 0, 1, 2, 3 };
        byte[] data2 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        Dictionary<byte[], byte[]> bloecke = new Dictionary<byte[], byte[]>() { { id1, data1 }, { id2, data2 } };
        var r = akteur.NachrichtBauen(0, bloecke);

        (var nachr_id, var r_) = Minformat.Akteur.NachrichtZerlegen(r);

        Assert.True(r_.Where(e => id1.SequenceEqual(e.Key)).Count() == 1);
        Assert.True(r_.Where(e => id2.SequenceEqual(e.Key)).Count() == 1);
        Assert.True(r_.Where(e => data1.SequenceEqual(e.Value)).Count() == 1);
        Assert.True(r_.Where(e => data2.SequenceEqual(e.Value)).Count() == 1);

        var item = r_.ElementAt(0);
        Assert.True(item.Key.SequenceEqual(id1));
        Assert.True(item.Value.SequenceEqual(data1));

        item = r_.ElementAt(1);
        Assert.True(item.Key.SequenceEqual(id2));
        Assert.True(item.Value.SequenceEqual(data2));
    }

    [Fact]
    public void Test3()
    {
        var akteur = new Minformat.Akteur(crc32Aktiv: false);
        byte[] id1 = { 1, 0, 0, 0, 4, 5, 6, 7 };
        byte[] data1 = { 9, 8, 7, 6, 5, 4, 3, 1, 0 };

        byte[] id2 = { 255, 0, 0, 0, 1, 2, 3, 4 };
        byte[] data2 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        Dictionary<byte[], byte[]> bloecke = new Dictionary<byte[], byte[]>() { { id1, data1 }, { id2, data2 } };
        var r = akteur.NachrichtBauen(0, bloecke);

        (var nachr_id, var r_) = Minformat.Akteur.NachrichtZerlegen(r);

        Assert.True(r_.Where(e => id1.SequenceEqual(e.Key)).Count() == 1);
        Assert.True(r_.Where(e => id2.SequenceEqual(e.Key)).Count() == 1);
        Assert.True(r_.Where(e => data1.SequenceEqual(e.Value)).Count() == 1);
        Assert.True(r_.Where(e => data2.SequenceEqual(e.Value)).Count() == 1);

        var item = r_.ElementAt(0);
        Assert.True(item.Key.SequenceEqual(id1));
        Assert.True(item.Value.SequenceEqual(data1));

        item = r_.ElementAt(1);
        Assert.True(item.Key.SequenceEqual(id2));
        Assert.True(item.Value.SequenceEqual(data2));
    }
}