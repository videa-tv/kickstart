using System;

namespace Kickstart.Pass2.CModel.Code
{
    public enum CAccessModifier
    {
        Public,
        Protected,
        Private,
        Internal
    }

    public static class CAccessModifierExtensions
    {
        public static string GetString(this CAccessModifier accessModifier)
        {
            switch (accessModifier)
            {
                case CAccessModifier.Public:
                    return "public";
                case CAccessModifier.Private:
                    return "private";
                case CAccessModifier.Protected:
                    return "protected";
                case CAccessModifier.Internal:
                    return "internal";
            }
            throw new NotImplementedException();
        }
    }
}