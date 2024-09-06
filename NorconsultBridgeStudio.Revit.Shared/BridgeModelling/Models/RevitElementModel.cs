using Autodesk.Revit.DB;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Models
{
    public class RevitElementModel
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }
        public RevitElementModel(ElementId id, string name)
        {
            Id = id;
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}