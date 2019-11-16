namespace Switchboard.Controllers.WebSocketsX.Facilities.Buffers
{
    internal interface IObjectOwner<in T>
    {
        void Return(T obj);
    }
}