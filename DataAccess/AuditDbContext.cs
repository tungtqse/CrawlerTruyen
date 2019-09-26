using Core;
using DataAccess.Models;
using EntityFramework.DbContextScope.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataAccess
{
    public class AuditDbContext : DbContext, IDbContext
    {
        readonly string TransactionId;

        public AuditDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            TransactionId = Convert.ToString(Guid.NewGuid());
        }

        public override int SaveChanges()
        {

            var list = new List<AuditTrail>();
            Guid currentUserId = new Guid("DFD60091-FD1B-416C-A390-1C829B1152D8");
            var currentDateTime = DateTime.Now;
            var dbset = this.Set<AuditTrail>();

            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

            if (modifiedEntries != null)
            {
                foreach (var entry in modifiedEntries)
                {
                    Type entity = entry.Entity.GetType();

                    if (entry.State == EntityState.Added)
                    {
                        #region Add

                        var status = entity.GetProperty(Constants.BaseProperty.StatusId).GetValue(entry.Entity, null);
                        if (status == null || status.Equals(false))
                            entity.GetProperty(Constants.BaseProperty.StatusId).SetValue(entry.Entity, true, null);

                        XElement xml = new XElement("Create");
                        var itemId = (Guid)entity.GetProperty(Constants.AuditTrailProperty.Id).GetValue(entry.Entity, null);
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

                        list.Add(auditTrail);

                        #endregion
                    }
                    else
                    {
                        #region Modify

                        #region Config XML
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

                        #endregion

                        // Create instance of AuditTrail for edit item
                        // var instanceAuditTrail = Activator.CreateInstance(auditType);
                        var itemId = (Guid)entity.GetProperty(Constants.AuditTrailProperty.Id).GetValue(entry.Entity, null);
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
                        list.Add(auditTrail);

                        #endregion
                    }

                    #region Update System Fields

                    if (entry.State == EntityState.Added
                        && entity.GetProperty(Constants.BaseProperty.CreatedBy) != null && entity.GetProperty(Constants.BaseProperty.CreatedDate) != null
                        && entity.GetProperty(Constants.BaseProperty.ModifiedBy) != null && entity.GetProperty(Constants.BaseProperty.ModifiedDate) != null)
                    {
                        entity.GetProperty(Constants.BaseProperty.CreatedDate).SetValue(entry.Entity, currentDateTime, null);
                        entity.GetProperty(Constants.BaseProperty.CreatedBy).SetValue(entry.Entity, currentUserId, null);
                        entity.GetProperty(Constants.BaseProperty.ModifiedDate).SetValue(entry.Entity, currentDateTime, null);
                        entity.GetProperty(Constants.BaseProperty.ModifiedBy).SetValue(entry.Entity, currentUserId, null);
                    }
                    else if (entry.State == EntityState.Modified
                        && entity.GetProperty(Constants.BaseProperty.ModifiedBy) != null && entity.GetProperty(Constants.BaseProperty.ModifiedDate) != null)
                    {
                        entity.GetProperty(Constants.BaseProperty.ModifiedDate).SetValue(entry.Entity, currentDateTime, null);
                        entity.GetProperty(Constants.BaseProperty.ModifiedBy).SetValue(entry.Entity, currentUserId, null);
                    }

                    #endregion
                }
            }

            try
            {
                var result = base.SaveChanges();

                try
                {
                    dbset.AddRange(list);

                    base.SaveChanges();
                }
                catch (Exception e)
                {

                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
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
    }
}
