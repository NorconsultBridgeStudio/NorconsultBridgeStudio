using Autodesk.Revit.DB;
using System;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Utils
{
    public class ExtensibleStorageUtils
    {
        private const string VendorId = "Norconsult";
        private const string SchemaName = "Alignment";
        private const string OriginFieldName = "AlignmentOrigin";
        private const string SchemaGuid = "C46CF039-BF32-448D-A712-7E093AFCB7EF";
        public static Schema CreateSchema()
        {
            Schema schema = null;

            SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid(SchemaGuid));
            schemaBuilder.SetReadAccessLevel(AccessLevel.Vendor);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
            schemaBuilder.SetVendorId(VendorId); // required because of restricted write-access
            schemaBuilder.SetSchemaName(SchemaName);

            FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField(OriginFieldName, typeof(XYZ));
            fieldBuilder.SetSpec(SpecTypeId.Length);
            fieldBuilder.SetDocumentation("A stored location value of the alignment origin in a view section");

            schema = schemaBuilder.Finish();

            return schema;
        }
        public static Schema GetSchema()
        {
            Schema schema = Schema.Lookup(new Guid(SchemaGuid));
            if (schema == null)
                schema = CreateSchema();

            return schema;
        }

        public static void StoreOrigin(Element element, XYZ origin)
        {
            Schema schema = GetSchema();
            Entity entity = new Entity(schema);
            Field originField = schema.GetField(OriginFieldName);

            entity.Set<XYZ>(originField, origin, UnitTypeId.Feet);
            element.SetEntity(entity);

        }
        public static XYZ RestoreOrigin(Element element)
        {
            Schema schema = Schema.Lookup(new Guid(SchemaGuid));
            if (schema == null)
                return null;

            Entity entity = element.GetEntity(schema);
            XYZ origin = entity.Get<XYZ>(schema.GetField(OriginFieldName), UnitTypeId.Feet);

            return origin;
        }
    }
}
