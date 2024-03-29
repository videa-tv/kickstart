using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject.Table
{
    public class CViewToCClassConverter
    {
        #region Methods

        public CClass Convert(CView view)
        {
            var @class = new CClass(view.ViewName);
            @class.Namespace = new CNamespace {NamespaceName = view.Schema.SchemaName};
           

            foreach (var column in view.Column)
            {
                var prop = new CProperty();
                prop.PropertyName = column.ColumnName;
                prop.Type = column.ColumnType.ToClrTypeName();
                @class.Property.Add(prop);
            }

            return @class;
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