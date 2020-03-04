using System.Text;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CFile : CPart
    {
        public CFile()
        {
            Folder = string.Empty;
        }


        public string FileName { get; set; }
        public string Folder { get; set; }

        public string Path => System.IO.Path.Combine(Folder, FileName);

        public string WrittenToPath { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}