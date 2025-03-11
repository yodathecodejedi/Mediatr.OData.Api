using Mediatr.OData.Api.Attributes;
using Mediatr.OData.Api.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mediatr.OData.Api.Example.DomainObjects;

//[JsonConverter(typeof(JsonStringEnumConverter))]
[Flags]
public enum Rechten
{
    None = 0,
    Read = 1 << 0,                         //1  
    Write = 1 << 1,                        //2
    Delete = 1 << 2,                       //4
    All = Read | Write | Delete            //7
}

public enum Test32
{
    A = 1, B, C, D, E, F, G
}

[Flags]
public enum FlagTest
{
    None = 0,
    A = 1 << 0,                         //1
    B = 1 << 1,                         //2
    C = 1 << 2,                         //4
    D = 1 << 3,                         //8
    E = 1 << 4,                         //16
    F = 1 << 5,                         //32
    //All = A | B | C | D | E | F         //63
}

//In processing 
//Als Key gevonden maar er is geen Propertymode dan is het Ignored ALL
//Als Gevonden dan toepassen
//ALs niet gevonden dan proberen op basis van navigationProperties 
[ObjectAuthorize]
public class Afdeling : IDomainObject<int>
{
    [Key]
    //[PropertyMode(Mode = Mode.Ignored, Operation = OperationType.All)]
    public int Sleutel { get; set; }

    [PropertyHash]
    [PropertyInternal]
    public string? Hash { get; set; } = Guid.NewGuid().ToString();

    //[PropertyMode(Mode = Mode.Allowed, Operation = OperationType.All)]
    public string? Name { get; set; }

    //[PropertyMode(Mode = Mode.Allowed, Operation = OperationType.All)]
    public string? Description { get; set; }

    //[PropertyMode(Mode = Mode.Allowed, Operation = OperationType.All)]
    public DateTimeOffset? Datum { get; set; }

    public DateTimeOffset Datum2 { get; set; }
    public DateTime? Datum3 { get; set; }

    public Test32? TestEnum { get; set; }

    public Rechten Rechten { get; set; } = Rechten.None;

    public FlagTest TestFlag { get; set; } //= FlagTest.None;

    public DateOnly DatumOnly { get; set; }
    public TimeOnly Time2 { get; set; }

    public Medewerker[]? Test { get; set; } = [];

    public List<Medewerker>? TestList { get; set; }

    public Bedrijf? Bedrijf { get; set; } = default!;

    public ICollection<Medewerker>? Medewerkers { get; set; } = [];
}