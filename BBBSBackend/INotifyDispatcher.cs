namespace BBBSBackend
{
    // En ideel løsning ville være at lave en liste over event i form af typer eller en enum, og dertil meddrage en generisk parameter datastruktur
    public interface INotifyDispatcher
    {
        Task DispatchBookingConfirmed(Guid bookingId);
        Task DispatchBookingConfirmedAdmin(Guid bookingId);
        Task DispatchBookingCanceled(Guid bookingId);
    }
}
