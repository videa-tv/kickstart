using System.IO;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass3.SqlServer
{
    public class SqlServerTableWriter : IWriter
    {
        #region Methods

        public void Write(CTable value, TextWriter textWriter)
        {
            //convert STable into Sql Server Create table DDL
            //write the file
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}