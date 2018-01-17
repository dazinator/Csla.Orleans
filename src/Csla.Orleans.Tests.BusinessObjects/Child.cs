﻿using System;
using System.Data;

namespace Csla.Orleans.Tests.BusinessObjects
{
    [Serializable()]
    public class Child : BusinessBase<Child>
    {
        private string _data = "";
        private Guid _guid = System.Guid.NewGuid();

        private GrandChildren _children = GrandChildren.NewGrandChildren();

        protected override object GetIdValue()
        {
            return _data;
        }

        public string Data
        {
            get { return _data; }
            set
            {
                if (_data != value)
                {
                    _data = value;
                    MarkDirty();
                }
            }
        }

#if !SILVERLIGHT
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Child))
            {
                return false;
            }

            return _data == ((Child)(obj))._data;
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }
#endif

        public Guid Guid
        {
            get { return _guid; }
        }

        public GrandChildren GrandChildren
        {
            get { return _children; }
        }

        internal static Child NewChild(string data)
        {
            Child obj = new Child();
            obj._data = data;
            return obj;
        }

        internal static Child GetChild(IDataReader dr)
        {
            Child obj = new Child();
            obj.Fetch(dr);
            return obj;
        }

        public Child()
        {
            //prevent direct creation
            MarkAsChild();
        }

        private void Fetch(IDataReader dr)
        {
            MarkOld();
        }

        internal void Update(IDbTransaction tr)
        {
            if (IsDeleted)
            {
                //we would delete here
                MarkNew();
            }
            else
            {
                if (IsNew)
                {
                    //we would insert here
                }
                else
                {
                    //we would update here
                }
                MarkOld();
            }
        }
    }
}
