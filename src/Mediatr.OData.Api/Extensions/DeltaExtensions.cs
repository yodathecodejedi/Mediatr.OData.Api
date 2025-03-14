using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Interfaces;
using Mediatr.OData.Api.Models;
using Microsoft.AspNetCore.OData.Deltas;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions;

public static class DeltaExtensions
{
    public static bool TryGetDomainObjectType<T>(this Delta<T> delta, out Type type) where T : class
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        type = default!;

        try
        {
            type = delta.GetType().GetGenericArguments()[0];
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetPropertyInfo<T>(this Delta<T> delta, string name, out PropertyInfo propertyInfo) where T : class
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));

        try
        {
            delta.TryGetDomainObjectType(out Type domainObjectType);
            PropertyInfo propInfo = domainObjectType.GetProperty(name) ?? default!;
            if (propInfo is null)
            {
                propertyInfo = default!;
                return false;
            }

            propertyInfo = propInfo;
            return true;
        }
        catch
        {
            propertyInfo = default!;
            return false;
        }
    }

    public static bool TryGetPropertyCategory<T>(this Delta<T> delta, string name, out PropertyCategory propertyCategory) where T : class
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        propertyCategory = PropertyCategory.Unknown;

        try
        {
            var hasPropertyInfo = delta.TryGetPropertyInfo(name, out PropertyInfo propInfo);
            if (!hasPropertyInfo)
                return false;
            var hasCategory = propInfo.TryGetPropertyCategory(out propertyCategory);
            if (!hasCategory)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetValidatedProperty<T>(this Delta<T> delta, string name, out ValidatedProperty validatedProperty) where T : class
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        validatedProperty = default!;

        try
        {
            var hasType = delta.TryGetPropertyType(name, out Type propType);
            if (!hasType)
                return false;
            var hasValue = delta.TryGetPropertyValue(name, out object propValue);
            if (!hasValue)
                return false;
            var hasInfo = delta.TryGetPropertyInfo(name, out PropertyInfo propInfo);
            if (!hasInfo)
                return false;
            var hasCategory = delta.TryGetPropertyCategory(name, out PropertyCategory propCategory);
            if (!hasCategory)
                return false;

            validatedProperty = new ValidatedProperty
            {
                Name = name,
                Mode = Mode.Required,       //TODO if Nullable dan op Allowed   //Op basis van FieldAttribute (Required maken op een nullable, bij actie post)
                Operation = HttpOperation.Patch, //Niet nodig als we hem goed bepalen
                Info = propInfo,
                Type = propType,
                Category = propCategory,
                Value = propValue
            };

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static T Post<T>(this Delta<T> delta) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        try
        {
            if (delta.TryPost(out T domainObject))
                return domainObject;
            return default!;
        }
        catch
        {
            return default!;
        }
    }

    public static bool TryPost<T>(this Delta<T> delta, out T domainObject) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));

        domainObject = default!;
        try
        {
            //We create a new instance of the domainObject
            domainObject = Activator.CreateInstance(typeof(T)) as T ?? default!;

            if (domainObject is null)
                return false;

            //Here we might need to do something with the Key ???

            delta.Patch(domainObject);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static T Patch<T>(this Delta<T> delta, T domainObject) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        ArgumentNullException.ThrowIfNull(domainObject, nameof(domainObject));
        try
        {
            if (delta.TryPatch(domainObject))
                return domainObject;
            else
                return default!;
        }
        catch
        {
            return default!;
        }
    }
    public static bool TryPatch<T>(this Delta<T> delta, T domainObject) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        ArgumentNullException.ThrowIfNull(domainObject, nameof(domainObject));

        try
        {
            delta.Patch(domainObject);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static T Put<T>(this Delta<T> delta, T domainObject) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        ArgumentNullException.ThrowIfNull(domainObject, nameof(domainObject));
        try
        {
            if (delta.TryPut(domainObject))
                return domainObject;
            else
                return default!;
        }
        catch
        {
            return default!;
        }
    }

    public static bool TryPut<T>(this Delta<T> delta, T domainObject) where T : class, IDomainObject
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));
        ArgumentNullException.ThrowIfNull(domainObject, nameof(domainObject));
        try
        {
            delta.Put(domainObject);
            return true;
        }
        catch
        {
            return false;
        }
    }


    public static bool ValidateModel<T>(this Delta<T> delta, ModelValidationMode validationMode) where T : class
    {
        ArgumentNullException.ThrowIfNull(delta, nameof(delta));

        bool isValid = true;
        //The actual Type of CLR
        delta.TryGetDomainObjectType(out Type domainObjectType);

        //The actual list of Properties including their Attributes
        PropertyInfo[] properties = domainObjectType.GetProperties();

        List<ValidatedProperty> validatedProperties = [];

        //supplied PropertyNames in the json
        List<string> suppliedProperties = [.. delta.GetChangedPropertyNames()];


        //Remove the key from the Delta  Patch / Post and Put the key should not be updated and a keyfield is not updateable
        var keyName = delta.GetKeyProperty().Name;

        //HIER MOETEN WE NOG EEN WRAPPER OBJECT validatedModel 
        //validatedProperties List<validatedProperty> 
        //IsValid 
        //HasKey
        //IsMissing
        //Failed 

        foreach (var suppliedProperty in suppliedProperties)
        {
            delta.TryGetValidatedProperty(suppliedProperty, out ValidatedProperty validatedProperty);

            //IF we can't validate a property but is has been changed we need to set the Delta<T> to failed
            if (validatedProperty is null)
            {
                isValid = false;
                break;
            }
            validatedProperty.IsKey = suppliedProperty.Equals(keyName);
            validatedProperties.Add(validatedProperty);
        }




        //Get the list of Updateable properties with the OperationType and Category and the Mode of the property
        //Include UpdateAble Propety
        //Name 
        //PropertyValue
        //IsMissing 
        //So we have all we need to generate the new Delta<T> object

        //Now we need to see if there is a value missing ...
        //If it is missing we need to set Delt<T> to Failed and set the FailedValidation to true
        //This way we prevent that the Patch / Post and Put will be executed
        //Check if it works ;)


        //Required, Ignored, Allowed, Object,Value,Navigation,Unknown, Post, Put, Patch, All

        //List of updateable properties



        //Remove the key from the Delta properties since at Patch / Post and Put the key should not be updated and a keyfield is not updateable
        var changeablePropertiesNew = delta.UpdatableProperties.ToList();

        //delta.CopyChangedValues();
        //delta.CopyUnchangedValues();
        //delta.ExpectedClrType = typeof(T);
        //delta.FailedValidation = false;
        //delta.Failed<T>("asdsa");
        //delta.GetChangedPropertyNames();
        //delta.GetDeltaNestedNavigationProperties();
        //delta.GetDynamicMemberNames();
        //delta.GetPropertyValue("asdsa"); 
        //delta.GetUnchangedPropertyNames();
        //delta.IsComplexType = false;
        //delta.Patch();
        //delta.Put();
        //delta.UpdatableProperties;
        //var delta = new Delta<T>();
        //var keyProperty = KeyProperty<T>();
        //var properties = Properties<T>();
        //foreach (var prop in properties)
        //{
        //    var pName = prop.Name;
        //    var oValue = prop.GetValue(original);
        //    var mValue = prop.GetValue(modified);
        //    if (pName.Equals(keyProperty.Name))
        //    {
        //        if (!oValue.Equals(mValue))
        //        {
        //            throw new Exception("Key value cannot be changed");
        //        }
        //        continue;
        //    }
        //    if (!oValue.Equals(mValue))
        //    {
        //        delta.TrySetPropertyValue(prop.Name, prop.GetValue(modified));
        //    }
        //}
        return isValid;
    }
}
