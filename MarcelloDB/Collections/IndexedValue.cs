﻿using System;
using System.Linq;
using System.Collections.Generic;
using MarcelloDB.Records;
using MarcelloDB.Serialization;
using MarcelloDB.Index;
using System.Collections;
using MarcelloDB.Index.BTree;
using MarcelloDB.Collections.Scopes;

namespace MarcelloDB.Collections
{
    public abstract class IndexedValue : SessionBoundObject
    {
        public IndexedValue(Session session) : base(session){}

        internal abstract IEnumerable<object> GetKeys(object o, Int64 address);

        protected internal abstract void Register(object o, Int64 address);

        protected internal abstract void UnRegister(object o, Int64 address);

        protected internal string PropertyName { get; set; }

        protected internal abstract void EnsureIndex();

        internal abstract void SetContext (Collection collection,
                         Session session,
                         RecordManager recordManager,
                         object serializer,
                         string propertyName);

        internal abstract object Build ();
    }
}