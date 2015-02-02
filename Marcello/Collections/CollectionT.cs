﻿using System;
using System.Collections.Generic;
using Marcello.Serialization;
using System.Linq;
using System.Threading.Tasks;
using Marcello.AllocationStrategies;
using Marcello.Records;
using Marcello.Storage;
using Marcello.Index;
using Marcello.Transactions.__;

namespace Marcello.Collections
{
    public class Collection{}

    public class Collection<T> : Collection
    {
        Marcello Session { get; set; }

        IObjectSerializer<T> Serializer { get; set; }

        IAllocationStrategy AllocationStrategy { get; set;}

        StorageEngine<T> StorageEngine {get;set;}

        RecordManager<T> RecordManager { get; set; }        		

        internal Collection (Marcello session, 
            IObjectSerializer<T> serializer,
            IAllocationStrategy allocationStrategy,
            StorageEngine<T> storageEngine)
        {
            Session = session;
            AllocationStrategy = allocationStrategy; 
            Serializer = serializer;
            StorageEngine = storageEngine;

            RecordManager = new RecordManager<T>(
                new DoubleSizeAllocationStrategy(),
                StorageEngine
            );                        
        }

        public IEnumerable<T> All
        {
            get{
                return new CollectionEnumerator<T>(this.Session, RecordManager, Serializer);
            }
        }            

        public T Find(object id)
        {
            T result = default(T);

            this.Session.Transaction(() =>
                {
                    var record =  GetRecordForObjectID(id);
                    if (record != null)
                    {
                        result = Serializer.Deserialize(record.Data);
                    }

                });
            return result;
        }

        public void Persist(T obj)
        {
            this.Session.Transaction(() => {
                PersistInternal(obj);                
            });               
        }            

        public void Destroy(T obj)
        {
            this.Session.Transaction(() => {
                DestroyInternal(obj);
            });
        }

        internal void DestroyAll()
        {
            var toDestroy = All.ToList();
            foreach(var o in toDestroy)
            {
                DestroyInternal(o);
            }        
        }

        internal void DisableJournal()
        {
            this.RecordManager.DisableJournal();
        }



        Record GetRecordForObjectID(object objectID)
        {                
            var index = RecordIndex.Create(this.RecordManager, RecordIndex.ID_INDEX_NAME);
            var address = index.Search(objectID);
            if (address > 0)
            {
                return RecordManager.GetRecord(address);
            }
            return null;
        }
            
        void PersistInternal(T obj)
        {
            var objectID = new ObjectProxy(obj).ID;
            //Try Load record with object ID
            Record record = GetRecordForObjectID(objectID);
            if (record != null) {
                UpdateObject(record, obj);
            }
            else {
                record = AppendObject(obj);
            }
                
            var index = RecordIndex.Create(this.RecordManager, RecordIndex.ID_INDEX_NAME);
            index.Register(objectID, record.Header.Address);
        }

        Record AppendObject(T obj)
        {
            var data = Serializer.Serialize(obj);
            return RecordManager.AppendRecord(data, true);
        }

        void UpdateObject(Record record, T obj)
        {
            var bytes = Serializer.Serialize(obj);
            RecordManager.UpdateRecord(record, bytes);
        }

        void DestroyInternal (T obj)
        {
            var objectID = new ObjectProxy(obj).ID;
            //Try Load record with object ID
            Record record = GetRecordForObjectID(objectID);
            if (record != null)
            {
                //release the record if present
                RecordManager.ReleaseRecord(record);

                var index = RecordIndex.Create(this.RecordManager, RecordIndex.ID_INDEX_NAME);
                index.UnRegister(objectID);
            }

        }
    }
}