using System;
using System.Data;

namespace Csla.Orleans.Tests.BusinessObjects
{
    [Serializable()]
    public class GrandChildren : BusinessBindingListBase<GrandChildren, GrandChild>
    {
        public void Add(string data)
        {
            this.Add(GrandChild.NewGrandChild(data));
        }

        internal static GrandChildren NewGrandChildren()
        {
            return new GrandChildren();
        }

        internal static GrandChildren GetGrandChildren(IDataReader dr)
        {
            //todo: load child data
            return null;
        }

        internal void Update(IDbTransaction tr)
        {
            foreach (GrandChild child in this)
            {
                child.Update(tr);
            }
        }

        public GrandChildren()
        {
            //prevent direct creation
            MarkAsChild();
        }
    }
}
