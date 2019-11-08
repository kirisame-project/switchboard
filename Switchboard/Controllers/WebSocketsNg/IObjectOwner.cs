namespace Switchboard.Controllers.WebSocketsNg
{
    internal interface IObjectOwner<in T>
    {
        void Return(T obj);
    }
}