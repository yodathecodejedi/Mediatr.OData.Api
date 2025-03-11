using Mediatr.OData.Api.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Mediatr.OData.Api.Serializers;

public class EnumResourceSerializer : ResourceSerializer
{
    public override void Process(SerializerResult serializerResult, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (serializerResult is null) return;
        if (selectExpandNode is null) return;
        if (resourceContext is null) return;
        if (serializerResult.Count == 0) return;

        foreach (ODataPropertyInfo oDataPropertyInfo in serializerResult.Remaining)
        {
            if (selectExpandNode.PropertyIsOfEdmKind(oDataPropertyInfo, EdmTypeKind.Enum))
            {
                selectExpandNode.TryGetStructuralProperty(oDataPropertyInfo, EdmTypeKind.Enum, out IEdmStructuralProperty structuralProperty);
                if (structuralProperty is null)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                serializerResult.Resource.TryGetODataProperty(oDataPropertyInfo, out ODataProperty oDataProperty);

                //If we don't get a oDataProperty we can't render anything  
                if (oDataProperty is null)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                //Type definitions are off so we cant rendr anything
                if (structuralProperty.Type.Definition is not IEdmEnumType typeDefinition)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                //If we don't get a suppliedValue we can't render anything
                if (oDataProperty.Value is not ODataEnumValue suppliedValue)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                //Normal Enumeration and the string value is in the members
                if (!typeDefinition.IsFlags && typeDefinition.Members.Select(m => m.Name).Contains(suppliedValue.ToString()))
                {
                    serializerResult.Add(oDataPropertyInfo);
                    continue;
                }

                //Normal Enumeration and the string value is not in the members we need to skip parsing it
                if (!typeDefinition.IsFlags && !typeDefinition.Members.Select(m => m.Name).Contains(suppliedValue.ToString()))
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                //Preprare to check the Flags version of Enumeration
                var hasAllDefined = typeDefinition.Members.Any(m =>
                                    m.Value.Value % 2 == 1 && m.Value.Value > 1);
                var hasNoneDefined = typeDefinition.Members.Any(m =>
                                    m.Value.Value == 0);
                var suppliedFlags = suppliedValue.Value.ToString().Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

                //Enum has no All value and all suppliedFlags exist so we can add it to the resultProperties (for now, maybe membercheck is needed)
                if (!hasAllDefined && suppliedFlags.All(typeDefinition.Members.Select(m => m.Name).Contains))
                {
                    serializerResult.Add(oDataPropertyInfo);
                    continue;
                }

                //Enum has no All value but not all suppliedFlags are in the enum so we skip it
                if (!hasAllDefined && !suppliedFlags.All(typeDefinition.Members.Select(m => m.Name).Contains))
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                //Get the EnumMember for All
                var allMember = typeDefinition.Members.FirstOrDefault(m => m.Value.Value % 2 == 1 && m.Value.Value > 1);

                var allMemberValue = allMember?.Value.Value ?? 0;
                var allMemberName = allMember?.Name ?? string.Empty;
                var suppliedMemberValue = typeDefinition.Members.Where(m => suppliedFlags.Contains(m.Name)).Sum(m => m.Value.Value);

                //Pattern               FlagTest.None | FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E | FlagTest.F | FlagTest.All;
                //Pattern               FlagTest.None | FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E | FlagTest.F;
                //Pattern               FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E | FlagTest.F | FlagTest.All;
                //Pattern               FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E | FlagTest.F;
                //Pattern               FlagTest.All
                //OdataSerializer       FlagTest.All
                //Renders               All

                //Early exit without any modifications needed
                if (suppliedMemberValue == allMemberValue)
                {
                    serializerResult.Add(oDataPropertyInfo);
                    continue;
                }

                //Pattern               !hasNoneDefined && suppliedMemberValue == 0  
                //oDataSerializer       Value = 0
                //Renders               Empty Enum 
                //Correction            Don't render
                if (!hasNoneDefined && suppliedMemberValue == 0)
                {
                    serializerResult.Remove(oDataPropertyInfo.Name);
                    continue;
                }

                //Pattern               FlagTest.None; && hasNoneDefined && suppliedMemberValue == 0
                //oDataSerializer       FlagTest.None; 
                //Renders               None
                if (hasNoneDefined && suppliedMemberValue == 0)
                {
                    serializerResult.Add(oDataPropertyInfo);
                    continue;
                }


                //Pattern               FlagTest.None | FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E;
                //Pattern               FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E;
                //oDataSerializer       FlagTest.A | FlagTest.B | FlagTest.C | FlagTest.D | FlagTest.E | FlagTest.All;
                //Renders               "A, B, C, D, E, All"
                //Correction            Remove All from output
                if (suppliedMemberValue > allMemberValue)
                {
                    string newValue = string.Join(",", suppliedFlags.Where(m => !m.Equals(allMemberName)));
                    serializerResult.Add(new ODataProperty { Name = oDataPropertyInfo.Name, Value = new ODataEnumValue(newValue) });
                    continue;
                }

                //If we reach this position we can't render the value
                serializerResult.Remove(oDataPropertyInfo.Name);
            }
        }
    }
}
