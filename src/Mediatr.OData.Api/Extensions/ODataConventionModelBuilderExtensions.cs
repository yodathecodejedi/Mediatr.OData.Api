using Mediatr.OData.Api.Abstractions.Attributes;
using Mediatr.OData.Api.Enumerations;
using Mediatr.OData.Api.Metadata;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Data;
using System.Reflection;

namespace Mediatr.OData.Api.Extensions
{
    public static class ODataConventionModelBuilderExtensions
    {
        public static bool TryAddNavigationProperty(this ODataConventionModelBuilder modelBuilder, EndpointMetadata metadata, EntityTypeConfiguration domainObjectTypeConfiguration, EntityTypeConfiguration navigationObjectTypeConfiguration)
        {
            if (modelBuilder is null) throw new ArgumentNullException(nameof(modelBuilder), "ODataConventionModelBuilder cannot be null.");
            if (metadata is null) throw new ArgumentNullException(nameof(metadata), "EndpointMetadata cannot be null.");
            if (domainObjectTypeConfiguration is null) throw new ArgumentNullException(nameof(domainObjectTypeConfiguration), "EntityTypeConfiguration cannot be null.");
            if (navigationObjectTypeConfiguration is null) throw new ArgumentNullException(nameof(navigationObjectTypeConfiguration), "EntityTypeConfiguration cannot be null.");

            try
            {
                Type clrDomainObjectType = metadata.DomainObjectType;
                Type clrNavigationObjectType = metadata.NavigationObjectType;

                PropertyInfo[] clrDomainObjectProperties = clrDomainObjectType.GetProperties();
                var validProperties = clrDomainObjectProperties
                    .Where(prop =>
                        !prop.GetCustomAttributes<ODataETagAttribute>().Any() &&
                        !prop.GetCustomAttributes<ODataIgnoreAttribute>().Any()
                    );

                var singleNavigations = validProperties.Where(prop => clrNavigationObjectType.IsAssignableFrom(prop.PropertyType));
                foreach (var singleNavigation in singleNavigations)
                {
                    domainObjectTypeConfiguration.AddNavigationProperty(singleNavigation, EdmMultiplicity.One);
                }

                // For a collection navigation
                var collectionNavigations = validProperties.Where(prop =>
                    (
                        prop.PropertyType.IsGenericType &&
                        typeof(IEnumerable<>).IsAssignableFrom(prop.PropertyType.GetGenericTypeDefinition()) &&
                        clrNavigationObjectType.IsAssignableFrom(prop.PropertyType.GetGenericArguments()[0])
                    ) ||
                    (
                        prop.PropertyType.IsArray &&
                        clrNavigationObjectType.IsAssignableFrom(prop.PropertyType.GetElementType())
                    )
                );
                foreach (var collectionNavigation in collectionNavigations)
                {
                    domainObjectTypeConfiguration.AddNavigationProperty(collectionNavigation, EdmMultiplicity.Many);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"The navigation property for {navigationObjectTypeConfiguration.Name} could not be added.", ex);
            }
            return true;
        }

        public static bool TryAddEntityset(this ODataConventionModelBuilder modelBuilder, EndpointMetadata metadata, EntityTypeConfiguration domainObjectTypeConfiguration, out EntitySetConfiguration domainSetTypeConfiguration)
        {
            domainSetTypeConfiguration = default!;
            if (modelBuilder is null) throw new ArgumentNullException(nameof(modelBuilder), "ODataConventionModelBuilder cannot be null.");
            if (metadata is null) throw new ArgumentNullException(nameof(metadata), "EndpointMetadata cannot be null.");
            if (domainObjectTypeConfiguration is null) throw new ArgumentNullException(nameof(domainObjectTypeConfiguration), "EntityTypeConfiguration cannot be null.");

            domainSetTypeConfiguration = modelBuilder.AddEntitySet(metadata.Route, domainObjectTypeConfiguration);
            if (domainSetTypeConfiguration is null)
            {
                throw new InvalidOperationException($"The entity set configuration for {metadata.Route} cannot be null.");
            }
            return true;
        }

        public static bool TryAddDomainObject(this ODataConventionModelBuilder modelBuilder, EndpointMetadata metadata, DomainObjectType domainObjectType, out EntityTypeConfiguration domainObjectTypeConfiguration)
        {
            domainObjectTypeConfiguration = default!;
            if (modelBuilder is null) throw new ArgumentNullException(nameof(modelBuilder), "ODataConventionModelBuilder cannot be null.");
            if (metadata is null) throw new ArgumentNullException(nameof(metadata), "EndpointMetadata cannot be null.");

            Type clrObjectType = domainObjectType == DomainObjectType.DomainObject ? metadata.DomainObjectType : metadata.NavigationObjectType;
            domainObjectTypeConfiguration = modelBuilder.AddEntityType(clrObjectType);
            if (!clrObjectType.TryGetKeyProperty(out var keyProperty) && !domainObjectTypeConfiguration.Keys.Any())
            {
                //Missing keyproperty
                throw new MissingPrimaryKeyException($"The key property is not declared in {clrObjectType.FullName}. Please use ODataKeyAttribute on the key property.");
            }
            //Set the key property
            domainObjectTypeConfiguration.HasKey(keyProperty);

            PropertyInfo[] clrObjectProperties = clrObjectType.GetProperties();

            //Get the properties we need to exclude from the EDM model (e.g. ODataIgnore, ODataETag) 
            var propertiesToBeIgnored = clrObjectProperties
                //Exclude the KeyProperty from the list of properties to be ignored just in case it was an Implicit Key 
                .Where(prop => !prop.Name.Equals(keyProperty.Name))
                //Select the following Attributes ODataIgnore, ODataETag
                .Where(prop =>
                    prop.GetCustomAttributes<ODataETagAttribute>().Any() ||
                    prop.GetCustomAttributes<ODataIgnoreAttribute>().Any());

            foreach (var propertyToBeIgnored in propertiesToBeIgnored)
            {
                //Remove the property from the EDM model, so it is not returned from OData
                domainObjectTypeConfiguration.RemoveProperty(propertyToBeIgnored);
            }

            //Add the enumaration properties to the EDM model
            var enumProperties = clrObjectProperties
                .Where(prop => prop.PropertyType.IsEnum)
                .ToList();
            foreach (var property in enumProperties)
            {
                //Exclude the properties that are already in the list of properties to be ignored
                if (!propertiesToBeIgnored.Any(prop => prop.Name.Equals(property.Name)))
                {
                    modelBuilder.AddEnumType(property.PropertyType);
                }
            }
            return true;
        }
    }
}
