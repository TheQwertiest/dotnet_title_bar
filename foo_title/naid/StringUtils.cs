using System;
using System.Collections.Generic;
using System.Text;

namespace naid {

    public static class StringUtils {

        public static String Join(string separator, IEnumerable<String> parts) {
            bool firstDone = false;

            StringBuilder builder = new StringBuilder();

            foreach (string part in parts) {
                if (firstDone)
                    builder.Append(separator);

                firstDone = true;

                builder.Append(part);
            }

            return builder.ToString();
        }
    }

}
