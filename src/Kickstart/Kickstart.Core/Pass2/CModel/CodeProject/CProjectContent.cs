using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CProjectContent : CPart
    {
        public CPart Content { get; set; }

        public CInterface Interface
        {
            get => Content as CInterface;
            set => Content = value;
        }

        public CClass Class
        {
            get => Content as CClass;
            set => Content = value;
        }

        public CAssemblyInfo AssemblyInfo
        {
            get => Content as CAssemblyInfo;
            set => Content = value;
        }

        public CFile File { get; set; }
        public CBuildAction BuildAction { get; set; } = CBuildAction.Compile;
        public bool CopyToOutputDirectory { get; set; } = false;
        public bool CopyToPublishDirectory { get; set; } = false;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitSProjectContent(this);
        }

        public override string ToString()
        {
            if (File != null)
                return File.FileName;
            if (Content != null)
                return Content.GetType().ToString();
            return base.ToString();
        }
    }
}