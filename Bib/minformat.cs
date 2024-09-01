using System.ComponentModel;
using System.Linq;

namespace Minformat {
    public class Akteur
    {
        public bool crc32Aktiv; //Flag ob crc32 durchgeführt und angehängt werden soll
        public uint id_block_laenge; //Anzahl der Bytes, die für die ID-Blöcke verwendet werden
        public uint nutzlast_block_blocklaenge; //Anzahl der Bytes, die verwendet werden, um die Angabe der Länge der Nutzlast in Bytes durchzuführen

        public Akteur(uint LaengeDerIdsInBytes = 8, UInt32 maximaleLaengeNutzlast = UInt32.MaxValue, bool crc32Aktiv = true)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(LaengeDerIdsInBytes, (uint)16, "Maximale Länge der ID-Blöcke beträgt 16.");

            ArgumentOutOfRangeException.ThrowIfGreaterThan(maximaleLaengeNutzlast, UInt32.MaxValue, "Maximale Länge der Nutzlast beträgt " + UInt32.MaxValue + ".");

            if (maximaleLaengeNutzlast > UInt16.MaxValue)
            {
                this.nutzlast_block_blocklaenge = 4;
            }
            else
            {
                this.nutzlast_block_blocklaenge = 2;
            }

            this.crc32Aktiv = crc32Aktiv;
            this.id_block_laenge = LaengeDerIdsInBytes;
        }
        /* 
                [Flags]
                public enum ZaehlerAuswertung
                {
                    Identisch = 1 << 1,
                    Aufsteigend = 1 << 2,
                    ZuruecksetzenMoeglich = 1 << 3
                }; */

        /* Byte 1
        .......................7 6 5 4.....................................................3 2 1...............................................0
        [angabe der zu lesenden bytes für die id-blöcke.][angabe der zu lesenden bytes, um die länge eines datenblocks zu erhalten.][crc32 aktiv oder nicht] */

        public static (uint, Dictionary<byte[], byte[]>) NachrichtZerlegen(byte[] nachricht)
        {
            Dictionary<byte[], byte[]> r = [];

            byte[] nachrichtenId_bytes = new byte[] { nachricht[0], nachricht[1], nachricht[2], nachricht[3] };
            uint nachrichtenId = BitConverter.ToUInt32(nachrichtenId_bytes);

            var crc32Aktiv = (nachricht[4] & 0b1) == 1;
            var id_block_laenge = (uint)(nachricht[4] >> 4 & 0b1111) + 1;
            var nutzlast_block_blocklaenge = (uint)(nachricht[4] >> 1 & 0b111) + 1;

            uint pos = 5;

            while (pos < nachricht.Length)
            {
                byte[] nutzlast_groesse = new byte[nutzlast_block_blocklaenge];
                //byte[] nutzlast_groesse = new byte[4];
                Array.Copy(nachricht, pos + id_block_laenge, nutzlast_groesse, 0, nutzlast_block_blocklaenge);

                UInt32 laenge_der_nutzlast = 0;
                if (nutzlast_block_blocklaenge == 4)
                {
                    laenge_der_nutzlast = BitConverter.ToUInt32(nutzlast_groesse);
                }
                else if (nutzlast_block_blocklaenge == 2)
                {
                    laenge_der_nutzlast = (UInt32)BitConverter.ToUInt16(nutzlast_groesse);
                } else {
                    throw new Exception("Der Block zur Längenangabe der Nutzlast ist nicht als 2 oder 4 Bytes lang deklariert.");
                }

                if (crc32Aktiv)
                {
                    var tmp = new byte[id_block_laenge + nutzlast_block_blocklaenge + laenge_der_nutzlast + 4];
                    Array.Copy(nachricht, pos, tmp, 0, tmp.Length-4);

                    var crc32 = new System.IO.Hashing.Crc32();
                    crc32.Append(tmp);
                    var crc32_hash = crc32.GetCurrentHash();

                    var nachricht_hash = new byte[4];
                    Array.Copy(nachricht, pos + tmp.Length - 4, nachricht_hash, 0, 4);

                    if (!crc32_hash.SequenceEqual(nachricht_hash))
                    {
                        pos += id_block_laenge + nutzlast_block_blocklaenge + laenge_der_nutzlast + 4;
                        continue;
                    }
                }

                byte[] id_block = new byte[id_block_laenge];
                Array.Copy(nachricht, pos, id_block, 0, id_block_laenge);

                byte[] nutzlast_block = new byte[laenge_der_nutzlast];
                Array.Copy(nachricht, pos + id_block_laenge + nutzlast_block_blocklaenge, nutzlast_block, 0, laenge_der_nutzlast);

                pos += id_block_laenge + nutzlast_block_blocklaenge + laenge_der_nutzlast;
                if (crc32Aktiv) pos += 4;

                r.Add(id_block, nutzlast_block);
            }

            return (nachrichtenId, r);
        }

        public byte[] NachrichtBauen(Dictionary<byte[], byte[]> bloecke)
        {
            return NachrichtBauen(0, bloecke, id_block_laenge, nutzlast_block_blocklaenge, crc32Aktiv);
        }

        public byte[] NachrichtBauen(Dictionary<byte[], byte[]> bloecke, bool crc32Aktiv)
        {
            return NachrichtBauen(0, bloecke, id_block_laenge, nutzlast_block_blocklaenge, crc32Aktiv);
        }

        public byte[] NachrichtBauen(uint nachrichtenId, Dictionary<byte[], byte[]> bloecke)
        {
            return NachrichtBauen(nachrichtenId, bloecke, id_block_laenge, nutzlast_block_blocklaenge, crc32Aktiv);
        }

        public byte[] NachrichtBauen(uint nachrichtenId, Dictionary<byte[], byte[]> bloecke, bool crc32Aktiv)
        {
            return NachrichtBauen(nachrichtenId, bloecke, id_block_laenge, nutzlast_block_blocklaenge, crc32Aktiv);
        }

        /* Byte 1
        .......................7 6 5 4.....................................................3 2 1...............................................0
        [angabe der zu lesenden bytes für die id-blöcke.][angabe der zu lesenden bytes, um die länge eines datenblocks zu erhalten.][crc32 aktiv oder nicht] */
        public static byte[] NachrichtBauen(uint nachrichtenId, Dictionary<byte[], byte[]> bloecke, uint id_block_laenge, uint nutzlast_block_blocklaenge, bool crc32Anhaengen)
        {
            ArgumentOutOfRangeException.ThrowIfZero(bloecke.Count, "Es wurden keine Daten übergeben.");

            ArgumentOutOfRangeException.ThrowIfGreaterThan(id_block_laenge, (uint)8, "Maximale Länge der ID-Blöcke beträgt 8.");

            ArgumentOutOfRangeException.ThrowIfGreaterThan(nutzlast_block_blocklaenge, (uint)8, "Maximale Länge der Nutznutzlast beträgt 8.");

            uint gesamtlaenge = 5;
            if (crc32Anhaengen)
            {
                foreach (var e in bloecke)
                {
                    gesamtlaenge += (uint)e.Value.Length + id_block_laenge + nutzlast_block_blocklaenge + 4;
                }
            }
            else
            {
                foreach (var e in bloecke)
                {
                    gesamtlaenge += (uint)e.Value.Length + id_block_laenge + nutzlast_block_blocklaenge;
                }
            }

            byte[] b = new byte[gesamtlaenge];

            var id = BitConverter.GetBytes(nachrichtenId);
            Array.Copy(id, 0, b, 0, id.Length);

            b[4] = (byte)((byte)((id_block_laenge - 1) << 4) + (byte)((nutzlast_block_blocklaenge - 1) << 1) + (crc32Anhaengen ? (byte)1 : (byte)0));

            int pos = 5;
            foreach (var e in bloecke)
            {
                var uebertragungsobjekt = Uebertragungsobjekt(e.Key, id_block_laenge, e.Value, nutzlast_block_blocklaenge, crc32Anhaengen);
                Array.Copy(uebertragungsobjekt, 0, b, pos, uebertragungsobjekt.Length);
                pos += uebertragungsobjekt.Length;
            }

            return b;
        }

        static byte[] Uebertragungsobjekt(byte[] id_block, uint id_block_laenge, byte[] nutzlast_block, uint nutzlast_block_blocklaenge, bool crc32Anhaengen)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(id_block_laenge, (uint)id_block.Length, "Der ID-Block ist zu lang.");

            var nutzlast_block_tatsaechlichelaenge = BitConverter.GetBytes(nutzlast_block.Length);

            int i = 0;
            while(nutzlast_block_tatsaechlichelaenge[i] != 0)
            {
                ++i;
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(nutzlast_block_blocklaenge, (uint)i, "Die Nutzlast ist zu lang.");

            var nachricht = crc32Anhaengen ? new byte[id_block_laenge + nutzlast_block_blocklaenge + nutzlast_block.Length + 4] : new byte[id_block_laenge + nutzlast_block_blocklaenge + nutzlast_block.Length];

            Array.Copy(id_block, 0, nachricht, 0, id_block.Length);
            Array.Copy(nutzlast_block_tatsaechlichelaenge, 0, nachricht, id_block_laenge, i);
            Array.Copy(nutzlast_block, 0, nachricht, id_block_laenge + nutzlast_block_blocklaenge, nutzlast_block.Length);

            if (crc32Anhaengen)
            {
                var crc32 = new System.IO.Hashing.Crc32();
                crc32.Append(nachricht);
                Array.Copy(crc32.GetCurrentHash(), 0, nachricht, id_block_laenge + nutzlast_block_blocklaenge + nutzlast_block.Length, 4);
            }

            return nachricht;
        }
    }
}
