namespace Minformat;

public class Akteur
{
    public bool crc32Aktiv = true;
    public uint id_block_blocklaenge = 12;
    public uint last_block_blocklaenge = 3;
    public uint zaehlerEingehend = 0;
    public uint zaehlerAusgehend = 0;

    Dictionary<byte[], byte[]> nachrichtZerlegen(byte[] nachricht)
    {
        Dictionary<byte[], byte[]> r = new Dictionary<byte[], byte[]>();

        byte[] zaehler_bytes = new byte[] { nachricht[0], nachricht[1], nachricht[2], nachricht[3] };
        uint zaehler = BitConverter.ToUInt32(zaehler_bytes);
        if (zaehler != 0 && zaehler < zaehlerEingehend) return null;

        if (zaehler == 0)
        {

        }

        var crc32Aktiv = (nachricht[4] & 0b1) == 1;
        var id_block_blocklaenge = (uint)(nachricht[4] >> 6 & 0b11) + 1;
        var last_block_blocklaenge = (uint)(nachricht[4] >> 4 & 0b11) + 1;

        uint pos = 5;


        while (pos < nachricht.Length)
        {
            byte[] last_groesse = new byte[last_block_blocklaenge];
            Array.Copy(nachricht, pos + id_block_blocklaenge, last_groesse, 0, last_block_blocklaenge);

            var laenge_der_last = BitConverter.ToUInt32(last_groesse);

            if (crc32Aktiv)
            {
                var tmp = new byte[id_block_blocklaenge + last_block_blocklaenge + laenge_der_last];
                Array.Copy(nachricht, pos, tmp, 0, tmp.Length);

                var crc32 = new System.IO.Hashing.Crc32();
                crc32.Append(tmp);

                var hash_ = new byte[4];
                Array.Copy(nachricht, pos + tmp.Length, hash_, 0, 4);

                if (crc32.GetCurrentHash() != hash_)
                {
                    pos += id_block_blocklaenge + last_block_blocklaenge + laenge_der_last + 4;
                    continue;
                }
            }

            byte[] id_block = new byte[id_block_blocklaenge];
            Array.Copy(nachricht, pos, id_block, 0, id_block_blocklaenge);

            byte[] last_block = new byte[laenge_der_last];
            Array.Copy(nachricht, pos + id_block_blocklaenge + last_block_blocklaenge, last_block, 0, laenge_der_last);

            pos += id_block_blocklaenge + last_block_blocklaenge + laenge_der_last;

            r.Add(id_block, last_block);
        }

        return r;
    }

    byte[] nachrichtBauen(Dictionary<byte[], byte[]> bloecke, int id_block_blocklaenge, int last_block_blocklaenge, bool crc32Anhaengen)
    {

    }

    byte[] nachrichtBauen(bool zaehlerAktiv, Dictionary<byte[], byte[]> bloecke, int id_block_blocklaenge, int last_block_blocklaenge, bool crc32Anhaengen)
    {

    }

    byte[] nachrichtBauen(uint zaehlerUeberschreiben, Dictionary<byte[], byte[]> bloecke, int id_block_blocklaenge, int last_block_blocklaenge, bool crc32Anhaengen)
    {

    }

    static byte[] nachrichtbauen(bool zaehlerAktiv, Dictionary<byte[], byte[]> bloecke, int id_block_blocklaenge, int last_block_blocklaenge, bool crc32Anhaengen)
    {
        if (bloecke.Count == 0) return null;

        var gesamtlaenge = 5;
        if (crc32Anhaengen)
        {
            foreach (var e in bloecke)
            {
                gesamtlaenge += e.Value.Length + id_block_blocklaenge + last_block_blocklaenge;
            }
        }
        else
        {
            foreach (var e in bloecke)
            {
                gesamtlaenge += e.Value.Length + id_block_blocklaenge + last_block_blocklaenge + 4;
            }
        }

        byte[] b = new byte[gesamtlaenge];

        if (zaehlerAktiv)
        {
            if (nachrichtenZaehler > uint.MaxValue)
            {
                nachrichtenZaehler = 0;
            }
            Array.Copy(BitConverter.GetBytes(nachrichtenZaehler), 0, b, 0, b.Length);
        }

        if (crc32Anhaengen)
        {
            b[4] = 1;
        }

        int pos = 5;
        foreach (var e in bloecke)
        {
            var uebertragungsobjekt = Uebertragungsobjekt(e.Key, id_block_blocklaenge, e.Value, last_block_blocklaenge, crc32Anhaengen);
            Array.Copy(uebertragungsobjekt, 0, b, pos, uebertragungsobjekt.Length);
        }

        return b;
    }

    static byte[] NachrichtBauen(byte[] type, int type_laenge, byte[] id, int id_laenge, byte[] last, int lastlaenge_laenge)
    {
        if (type_laenge < type.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }

        if (id_laenge < id.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }


        if (lastlaenge_laenge < last.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }

        var nachricht = new byte[type_laenge + id_laenge + lastlaenge_laenge + last.Length];

        var last_laenge = BitConverter.GetBytes(last.Length);

        Array.Copy(type, 0, nachricht, 0, type.Length);
        Array.Copy(id, 0, nachricht, type.Length, id.Length);
        Array.Copy(last_laenge, 0, nachricht, type.Length + id.Length, last_laenge.Length);
        Array.Copy(last, 0, nachricht, type.Length + id.Length + lastlaenge_laenge, last.Length);

        return nachricht;
    }

    static byte[] Uebertragungsobjekt(byte[] id_block, int id_block_blocklaenge, byte[] last_block, int last_block_blocklaenge, bool crc32Anhaengen)
    {
        if (id_block_blocklaenge < id_block.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }

        var last_block_tatsaechlichelaenge = BitConverter.GetBytes(last_block.Length);

        if (last_block_blocklaenge < last_block_tatsaechlichelaenge.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }

        var nachricht = crc32Anhaengen ? new byte[id_block_blocklaenge + last_block_blocklaenge + last_block.Length + 4] : new byte[id_block_blocklaenge + last_block_blocklaenge + last_block.Length];

        Array.Copy(id_block, 0, nachricht, 0, id_block.Length);
        Array.Copy(last_block_tatsaechlichelaenge, 0, nachricht, id_block_blocklaenge, last_block_tatsaechlichelaenge.Length);
        Array.Copy(last_block, 0, nachricht, id_block_blocklaenge + last_block_blocklaenge, last_block.Length);

        if (crc32Anhaengen)
        {
            var crc32 = new System.IO.Hashing.Crc32();
            crc32.Append(nachricht);
            Array.Copy(crc32.GetCurrentHash(), 0, nachricht, id_block_blocklaenge + last_block_blocklaenge + last_block.Length, 4);
        }

        return nachricht;
    }

    static byte[] DatennachrichtBauen(byte[] id, int id_laenge, byte[] last, int lastlaenge_laenge)
    {
        if (id_laenge < id.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }


        if (lastlaenge_laenge < last.Length)
        {
            throw new ArgumentOutOfRangeException("");
        }

        var nachricht = new byte[id_laenge + lastlaenge_laenge + last.Length];

        var last_laenge = BitConverter.GetBytes(last.Length);

        Array.Copy(id, 0, nachricht, 0, id.Length);
        Array.Copy(last_laenge, 0, nachricht, id.Length, last_laenge.Length);
        Array.Copy(last, 0, nachricht, id.Length + lastlaenge_laenge, last.Length);

        return nachricht;
    }
}
