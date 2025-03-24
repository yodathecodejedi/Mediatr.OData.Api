namespace Mediatr.OData.Example.DomainModel.Logistics
{
    public enum Status
    {
        Planned,
        PickingAndPackaging,
        ReadyToShip,
        WaitingForCarrier,
        HandedToCarrier,
        InTransit,
        AtCustoms,
        OutForDelivery,
        Delivered
    }
}
