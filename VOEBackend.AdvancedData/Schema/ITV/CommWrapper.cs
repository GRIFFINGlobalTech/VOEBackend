using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.AdvancedData.Schema.ITV
{
    public class CommWrapper
    {
        public Login Login { get; set;}
        public Order Order { get; set;}        
        public Borrower Borrower { get; set; }
        public Employer Employer { get; set; }
        public DocumentVoE DocumentVoE { get; set; }
    }
}
