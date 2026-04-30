using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.AdvancedData.Schema.ITV
{

    public class Login
    {
        public string ThirdPartyID { get; set; }
        public string ClientID { get; set; }
        public string Password { get; set; }
        public string Timestamp { get; set; }
        public string Method { get; set; }
        public string StatusUpdate { get; set; }

        public bool ShouldSerializeStatusUpdate()
        {
            return !StatusUpdate.Equals(String.Empty);
        }

        public class CommOperation
        {
            private CommOperation(string value) { Value = value; }

            public string Value { get; set; }

            public static CommOperation CreateOrder { get { return new CommOperation("voeorder.create"); } }
            public static CommOperation CheckStatus { get { return new CommOperation("voeorder.status"); } }

        }
    }
}
