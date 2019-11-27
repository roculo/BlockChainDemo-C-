using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainDemo
{
    public static class BlockChainExtension
    {
        public static byte[] GenerateHash(this IBlock block)
        {
            using (SHA512 sha = new SHA512Managed())
            using (MemoryStream st = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(st))
            {
                bw.Write(block.Data.ToString());
                bw.Write(block.Nonce);
                bw.Write(block.TimeStamp.ToBinary());
                bw.Write(block.PreHash);
                var starr = st.ToArray();
                return sha.ComputeHash(starr);
            }
        }
        public static byte[] MineHash(this IBlock block, byte[] difficulty)
        {
            if (difficulty == null) throw new ArgumentNullException(nameof(difficulty));
            byte[] hash = new byte[0];
            int d = difficulty.Length;
            while (!hash.Take(2).SequenceEqual(difficulty))
            {
                block.Nonce++;
                hash = block.GenerateHash();

            }
            return hash;
        }
        public static bool IsValid(this IBlock block)
        {
            var bk = block.GenerateHash();
            return block.Hash.SequenceEqual(bk);
        }
        public static bool IsValidPrevBlock(this IBlock block, IBlock prevBlock)
        {
            if (prevBlock == null) throw new ArgumentNullException(nameof(prevBlock));
            var prev = prevBlock.GenerateHash();
            return prevBlock.IsValid() && block.PreHash.SequenceEqual(prev);
        }
        public static bool IsValid(this IEnumerable<IBlock> items)
        {
            var enmerable = items.ToList();
            return enmerable.Zip(enmerable.Skip(1), Tuple.Create).All(block => block.Item2.IsValid() && block.Item2.IsValidPrevBlock(block.Item1));
        }
    }
    public class RecordRelationship
    {
        public String RRCref;
        public String ownership;
        public String AccessPolicies;
        public String Status;
        public String Si;

        public RecordRelationship(string rRCref, string ownership, string accessPolicies, string status, string si)
        {
            RRCref = rRCref;
            this.ownership = ownership;
            AccessPolicies = accessPolicies;
            Status = status;
            Si = si;
        }

        public override string ToString()
        {
            String result = "\n RRC ref:" + RRCref
                + "\n ownership:" + ownership
                + "\n Access Policies:" + AccessPolicies
            + "\n Status:" + Status
            + "\n Si:" + Si
           ;


            return result;
        }

    }

    public interface IBlock
    {
        RecordRelationship Data { get; }
        byte[] Hash { get; set; }
        int Nonce { get; set; }
        byte[] PreHash { get; set; }
        DateTime TimeStamp { get; }
    }

    public class Block : IBlock
    {
        public RecordRelationship Data { get; }
        public byte[] Hash { get; set; }
        public int Nonce { get; set; }
        public byte[] PreHash { get; set; }
        public DateTime TimeStamp { get; }

        public Block(RecordRelationship data)
        {
            Data = data;
            Nonce = 0;
            PreHash = new byte[] { 0x00 };
            TimeStamp = DateTime.Now;
        }
        public override string ToString()
        {
            return $"HASH: {BitConverter.ToString(Hash).Replace("-", "")}:\nHASH PREV: {BitConverter.ToString(PreHash).Replace("-", "")} \n Nonce: {Nonce} \n Time:{TimeStamp}" + Data.ToString();
        }
    }

    public class BlockChain : IEnumerable<IBlock>
    {
        private List<IBlock> _items = new List<IBlock>();
        public BlockChain(byte[] difficulty, IBlock genesis)
        {
            Difficulty = difficulty;
            genesis.Hash = genesis.MineHash(difficulty);
            Items.Add(genesis);
        }
        public void Add(IBlock item)
        {
                if (Items.LastOrDefault() != null)
                {
                    item.PreHash = Items.LastOrDefault()?.Hash;
                }
                item.Hash = item.MineHash(Difficulty);
                Items.Add(item);
        }
        public int Count => Items.Count;
        public IBlock this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }
        public List<IBlock> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
            }
        }
        public byte[] Difficulty { get; }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }

    public class BenhVien
    {
        public String tenBV;
        public BlockChain blockchain;
        public String ledge;

        public BenhVien(string tenBV, BlockChain blockchain, string ledge)
        {
            this.tenBV = tenBV;
            this.blockchain = blockchain;
            this.ledge = ledge;
        }
    }

    class Program
    {
        public static void XemThongTinNhaCungCap(BlockChain chain)
        {
            for (int i = 0; i < chain.Count; i++)
            {
                Console.WriteLine(chain[i].ToString() + "\n");
            }
            Console.ReadLine();
        }
        public static void ThemBlockThongTin(List<BenhVien> providers)
        {
            Console.WriteLine("Input RRC ref");
            var rrc = Console.ReadLine();
            Console.WriteLine("Input ownership");
            var ownership = Console.ReadLine();
            Console.WriteLine("Input Access Polices");
            var access = Console.ReadLine();
            Console.WriteLine("Input Status");
            var status = Console.ReadLine();
            Console.WriteLine("Input Si");
            var si = Console.ReadLine();
            int result = 0;
            for (int i = 0; i < providers.Count; i++)// đồng thuận phi tập trung 
            {
                Console.Write("Provider " + i + ":1-Yes 2-No ? ");//Lấy phiếu
                String val = Console.ReadLine();
                if (val == "1")
                {
                    result++;
                }
            }
            if (result > providers.Count / 2) // Phiếu >50%
            {
                for (int i = 0; i < providers.Count; i++)
                {
                    Block newBlock = new Block(new RecordRelationship(rrc, ownership, access, status, si));
                    providers[i].blockchain.Add(newBlock);
                }
                Console.WriteLine("Added, Votes >50%");
            }
            else //Phiếu <50%
            {
                Console.WriteLine("Canceled, Votes <50%");
            }
          
 

            Console.ReadLine();
        }
        public static void XemHoSoBenhNhan(List<BenhVien> providers,String name)
        {
            for(int i=0;i<providers.Count;i++)
            {
                Console.WriteLine("Benh Vien: "+providers[i].tenBV);
                var lst=providers[i].blockchain.Where(p => p.Data.ownership == name).ToList();
                if(lst.Count==0)
                {
                    Console.WriteLine("Khong co");
                }
                foreach(var x in lst)
                {
                   
                    Console.WriteLine(x.Data.ToString());
                    Console.WriteLine(x.TimeStamp);
                }
            }
        }
        public static bool MainMenu(List<BenhVien> providers)
        {
            Console.Clear();
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1) Chon Benh Vien");
            Console.WriteLine("2) Them Du Lieu");
            Console.WriteLine("3) Xem Ho So Benh Nhan");
            Console.WriteLine("Other to Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    {
                        Console.WriteLine("Chon Benh Vien:");
                        Console.WriteLine("1) Benh Vien Quoc Te");
                        Console.WriteLine("2) Benh Vien Thong Nhat");
                        Console.WriteLine("3) Benh Vien Sai Gon");
                        switch (Console.ReadLine())
                        {
                            case "1":
                                {
                                    XemThongTinNhaCungCap(providers[0].blockchain);
                                    return true;
                                }
                            case "2":
                                {
                                    XemThongTinNhaCungCap(providers[1].blockchain);
                                    return true;
                                }
                            case "3":
                                {
                                    XemThongTinNhaCungCap(providers[2].blockchain);
                                    return true;
                                }
                        }
                        return true;
                    }

                case "2":
                    {
                        ThemBlockThongTin(providers);
                        return true;
                    }

                case "3":
                    {
                        Console.WriteLine("\rNhap ten benh nhan");
                        String name = Console.ReadLine();
                        XemHoSoBenhNhan(providers,name);
                        Console.ReadLine();
                        return true;
                    }     
                default:
                    return false;
            }
        }

        static void Main(string[] args)
        {
            byte[] difficulty = new byte[] { 0x00, 0x00 };// Hash 
            // Benh Vien Quoc Te
            IBlock genesis = new Block(new RecordRelationship("1", "Bui Hai Duong", "public", "Normal", "GG"));
            BlockChain chain = new BlockChain(difficulty, genesis);
            chain.Add(new Block(new RecordRelationship("2", "Bui Hai Duong", "public", "Strong", "GS")));
            BenhVien bv1 = new BenhVien("Benh Vien Quoc Te", chain, "");

            // Benh Vien Thong Nhat
            IBlock genesis2 = new Block(new RecordRelationship("3", "Tran The Chau", "public", "Weak", "YY"));
            BlockChain chain2 = new BlockChain(difficulty, genesis2);
            chain2.Add(new Block(new RecordRelationship("4", "Tran The Chau", "public", "Average", "YT")));
            BenhVien bv2 = new BenhVien("Benh Vien Thong Nhat", chain2, "");

            // Benh Vien Sai Gon
            IBlock genesis3 = new Block(new RecordRelationship("5", "Luong Gia Kiet", "public", "Strong", "TT"));
            BlockChain chain3 = new BlockChain(difficulty, genesis3);
            chain3.Add(new Block(new RecordRelationship("6", "Luong Gia Kiet", "public", "Normal", "NM")));
            BenhVien bv3 = new BenhVien("Benh Vien Sai Gon", chain3, "");

            List<BenhVien> providers = new List<BenhVien>();// Danh sach lien ket
            providers.Add(bv1);
            providers.Add(bv2);
            providers.Add(bv3);

            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu(providers);
            }


            Console.ReadLine();
        }
    }
}
