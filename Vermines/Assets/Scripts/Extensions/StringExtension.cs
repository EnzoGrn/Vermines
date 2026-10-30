namespace Vermines.Extension
{

    public static class StringExtension
    {

        // Remplace WebSocketSharp.Ext.IsNullOrEmpty (la lib entière n'était tirée
        // que pour cette extension). Même signature, même comportement : évite
        // de toucher aux dizaines de call sites `truc.IsNullOrEmpty()`.
        public static bool IsNullOrEmpty(this string @this)
        {
            return string.IsNullOrEmpty(@this);
        }
    }
}