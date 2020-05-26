using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace AntiTamper
{
    public class MD5HashGenerator
    {
        public string GenerateHash(string filename)
        {
            FileStream file = System.IO.File.OpenRead(filename);                          //generating hash
            byte[] filebytes = new byte[file.Length];
            file.Read(filebytes, 0, filebytes.Length);

            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(filebytes);
            return Encoding.ASCII.GetString(hash, 0, hash.Length);
        }
    }
}
