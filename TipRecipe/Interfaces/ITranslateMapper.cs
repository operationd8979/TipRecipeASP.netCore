namespace TipRecipe.Interfaces
{
    public interface ITranslateMapper<T,U>
    {
        U Translate(T obj);
        T Translate(U obj);
        IList<U> TranslateList(IList<T> listObj);
        IList<T> TranslateList(IList<U> listObj);
        bool Compare(T a, U b);
    }
}
