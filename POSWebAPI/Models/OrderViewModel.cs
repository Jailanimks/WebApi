using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSWebAPI.Models
{
    public class OrderViewModel
    {
            public OrderViewModel()
            {
                this.OrderDetails = new List<OrderDetailsViewModel>();
            }

            public string PoNo { get; set; }

            public string SupLNo { get; set; }

            public DateTime? PoDate { get; set; }

            public string SuplName { get; set; }

            public string SuplAddr { get; set; }

            public virtual ICollection<OrderDetailsViewModel> OrderDetails { get; set; }
        }


        public class OrderDetailsViewModel
        {
            public string ItCode { get; set; }
            public string CPoNo { get; set; }
            public string ItDesc { get; set; }
            public int? Qty { get; set; }
            public decimal? ItRate { get; set; }

        }
    
    }