using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public static class IoTEncoder
    {
        /// <summary>
        ///     Codifica un texto
        /// </summary>
        /// <param name="txt">Texto a codificar</param>
        /// <returns>Texto codificado</returns>
        public static string EncodeString(string txt)
        {
            var cut = (txt.Length / 2) + (txt.Length % 2);
            var aux = txt.Substring(cut) + txt.Substring(0, cut);

            txt = BitConverter.ToString(Encoding.ASCII.GetBytes(aux));
            txt = txt.Replace("-", "");

            var charArray = txt.ToCharArray();

            Array.Reverse(charArray);
            txt = new string(charArray);
            return txt;
        }

        /// <summary>
        ///     Descodifica un texto
        /// </summary>
        /// <param name="txt">Texto a decodificar</param>
        /// <returns>Texto decodificado</returns>
        public static string DecodeString(string txt)
        {
            if (!string.IsNullOrEmpty(txt))
            {
                var sb = new StringBuilder();
                var charArray = txt.ToCharArray();

                Array.Reverse(charArray);
                txt = new string(charArray);

                for (int i = 0; i <= txt.Length - 2; i += 2)
                    sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(txt.Substring(i, 2), System.Globalization.NumberStyles.HexNumber))));

                txt = sb.ToString();
                int cut = (txt.Length / 2);

                return txt.Substring(cut) + txt.Substring(0, cut);
            }
            return string.Empty;
        }
    }
}
