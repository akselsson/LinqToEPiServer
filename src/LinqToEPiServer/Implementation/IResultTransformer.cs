namespace LinqToEPiServer.Implementation
{
    public interface IResultTransformer
    {
        object Transform(object input);
    }
}