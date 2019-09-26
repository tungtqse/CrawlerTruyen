using Core;
using DataAccess.Models;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly MainContext dbContext;
        readonly string TransactionId;
        readonly Dictionary<Type, IDbRepository> cachedRepositories = new Dictionary<Type, IDbRepository>();

        public UnitOfWork(MainContext dbContext)
        {
            this.dbContext = dbContext;
            this.TransactionId = Convert.ToString(Guid.NewGuid());
        }

        #region Contructor

        public MainContext GetDbContext()
        {
            return dbContext;
        }

        public IDbRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (cachedRepositories.ContainsKey(type))
            {
                return cachedRepositories[type] as IDbRepository<T>;
            }
            else
            {
                var repo = new DbRepository<T>(dbContext.Set<T>());
                cachedRepositories[type] = repo;

                return repo;
            }
        }

        private string GetTableName(DbEntityEntry ent)
        {
            ObjectContext objectContext = ((IObjectContextAdapter)this).ObjectContext;
            Type entityType = ent.Entity.GetType();

            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
                entityType = entityType.BaseType;

            string entityTypeName = entityType.Name;

            System.Data.Entity.Core.Metadata.Edm.EntityContainer container =
                objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace);
            string entitySetName = (from meta in container.BaseEntitySets
                                    where meta.ElementType.Name == entityTypeName
                                    select meta.Name).First();
            return entitySetName;
        }

        #endregion

        public void Dispose()
        {
            //dbContext.Dispose();
        }

        public Task<int> SaveChangeAsyncs()
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            Type type;
            var list = new List<DbEntityEntry>();
            Guid currentUserId = Guid.NewGuid(); //Constant.SystemId;
            var currentDateTime = DateTime.Now;

            #region Get AuditTrail

            // var auditType = dbContext
            // .GetType()
            // .GetProperties()
            // .Where(p =>
            //     p.PropertyType.IsGenericType &&
            //     p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            // .Select(p => p.PropertyType.GetGenericArguments()[0])
            // .FirstOrDefault(t => t.Name == "AuditTrail");
            var dbset = dbContext.Set<AuditTrail>();

            #endregion

            var changeSet = dbContext.ChangeTracker.Entries();
            changeSet.Select(t => new
            {
                Original = t.OriginalValues.PropertyNames.ToDictionary(pn => pn, pn => t.OriginalValues[pn]),
                Current = t.CurrentValues.PropertyNames.ToDictionary(pn => pn, pn => t.CurrentValues[pn]),
            });

            if (changeSet != null)
            {               

                var entries = changeSet.Where(f => f.State == EntityState.Added || f.State == EntityState.Modified);

                foreach (var entry in entries)
                {
                    type = entry.Entity.GetType();
                    var entityName = type.Name;

                    if (entry.State == EntityState.Modified)
                    {

                        #region Create AuditTrail

                        var originalValues = entry.OriginalValues.PropertyNames.ToDictionary(pn => pn, pn => entry.OriginalValues[pn]);
                        var currentValues = entry.CurrentValues.PropertyNames.ToDictionary(pn => pn, pn => entry.CurrentValues[pn]);

                        XElement xml = new XElement("Change");

                        foreach (var value in originalValues)
                        {
                            var oldValue = value.Value != null ? value.Value.ToString() : string.Empty;
                            var newValue = currentValues[value.Key] != null ? currentValues[value.Key].ToString() : string.Empty;

                            if (oldValue != newValue)
                            {
                                var field = new XElement("field");
                                var att = new XAttribute("Name", value.Key);
                                field.Add(att);

                                var oldNode = new XElement("OldValue", oldValue);
                                var newNode = new XElement("NewValue", newValue);
                                field.Add(oldNode);
                                field.Add(newNode);
                                xml.Add(field);
                            }
                        }

                        // Create instance of AuditTrail for edit item
                        // var instanceAuditTrail = Activator.CreateInstance(auditType);
                        var itemId = (Guid)type.GetProperty(Constants.AuditTrailProperty.Id).GetValue(entry.Entity, null);
                        var datatable = GetTableName(entry);

                        var auditTrail = new AuditTrail()
                        {
                            ItemId = itemId,
                            TableName = datatable,
                            ModifiedDate = currentDateTime,
                            ModifiedBy = currentUserId,
                            TrackChange = xml.ToString(),
                            TransactionId = TransactionId,
                            StatusId = true,
                            CreatedDate = currentDateTime,
                            CreatedBy = currentUserId
                        };

                        // Insert AuditTrail
                        dbset.Add(auditTrail);

                        #endregion
                    }
                    else
                    {
                        #region Set status as active/inactive
                        var status = type.GetProperty(Constants.BaseProperty.StatusId).GetValue(entry.Entity, null);
                        if (status == null || status.Equals(false))
                            type.GetProperty(Constants.BaseProperty.StatusId).SetValue(entry.Entity, true, null);
                        list.Add(entry);
                        #endregion
                    }

                    #region Update System Fields

                    if (entry.State == EntityState.Added
                        && type.GetProperty(Constants.BaseProperty.CreatedBy) != null && type.GetProperty(Constants.BaseProperty.CreatedDate) != null
                        && type.GetProperty(Constants.BaseProperty.ModifiedBy) != null && type.GetProperty(Constants.BaseProperty.ModifiedDate) != null)
                    {
                        type.GetProperty(Constants.BaseProperty.CreatedDate).SetValue(entry.Entity, currentDateTime, null);
                        type.GetProperty(Constants.BaseProperty.CreatedBy).SetValue(entry.Entity, currentUserId, null);
                        type.GetProperty(Constants.BaseProperty.ModifiedDate).SetValue(entry.Entity, currentDateTime, null);
                        type.GetProperty(Constants.BaseProperty.ModifiedBy).SetValue(entry.Entity, currentUserId, null);
                    }
                    else if (entry.State == EntityState.Modified
                        && type.GetProperty(Constants.BaseProperty.ModifiedBy) != null && type.GetProperty(Constants.BaseProperty.ModifiedDate) != null)
                    {
                        type.GetProperty(Constants.BaseProperty.ModifiedDate).SetValue(entry.Entity, currentDateTime, null);
                        type.GetProperty(Constants.BaseProperty.ModifiedBy).SetValue(entry.Entity, currentUserId, null);
                    }

                    #endregion
                }
            }

            #region Save

            try
            {
                #region  Set & Save AuditTrail for new item

                var result = dbContext.SaveChanges();

                try
                {
                    foreach (var entry in list)
                    {
                        type = entry.Entity.GetType();
                        var entityName = type.Name;
                        XElement xml = new XElement("Create");

                        // var instanceAuditTrail = Activator.CreateInstance(auditType);
                        var itemId = (Guid)type.GetProperty(Constants.AuditTrailProperty.Id).GetValue(entry.Entity, null);
                        var datatable = GetTableName(entry);
                        var auditTrail = new AuditTrail()
                        {
                            ItemId = itemId,
                            TableName = datatable,
                            ModifiedDate = currentDateTime,
                            ModifiedBy = currentUserId,
                            TrackChange = xml.ToString(),
                            TransactionId = TransactionId,
                            StatusId = true,
                            CreatedDate = currentDateTime,
                            CreatedBy = currentUserId
                        };

                        // Insert AuditTrail
                        dbset.Add(auditTrail);
                    }

                    dbContext.SaveChanges();

                }
                catch (Exception ex) { }

                return result;

                #endregion
            }
            catch (ValidationException ex)
            {
                Console.WriteLine("Error: ", ex.Message);
                throw;
            }

            #endregion
        }

        public virtual UnitOfWork Create()
        {
            var ctx = Activator.CreateInstance(typeof(MainContext)) as MainContext;
            return Activator.CreateInstance(typeof(UnitOfWork), ctx) as UnitOfWork;
        }
    }
}
