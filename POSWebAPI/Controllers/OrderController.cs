using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using POSWebAPI.Models;
using System.Web.Http.Description;
using System.Web.Http.Cors;

namespace POSWebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
  
    public class OrderController : ApiController
    {
        private FSDEntities db = new FSDEntities();

        // GET api/Order  
        public IHttpActionResult GetAllorders()
        {
            IList<OrderViewModel> orders = null;
            
            db.POMASTERs.Include(c => c.SUPPLIER);
            db.PODETAILs.Include(c => c.ITEM);
            db.POMASTERs.Include(db => db.PODETAILs);

                        

            using (var ctx = new FSDEntities())
            {
                orders = ctx.POMASTERs.Select(s => new OrderViewModel()
                {
                    PoNo = s.PONO,
                    PoDate = s.PODATE,
                    SupLNo = s.SUPLNO,
                    SuplName = s.SUPPLIER.SUPLNAME,
                    SuplAddr = s.SUPPLIER.SUPLADDR,
                    OrderDetails = ctx.PODETAILs.Where(s1 => s1.PONO == s.PONO).Select(s1 => new OrderDetailsViewModel()
                    {
                        ItCode = s1.ITCODE,
                        CPoNo = s1.PONO,
                        ItDesc = s1.ITEM.ITDESC,
                        Qty = s1.QTY,
                        ItRate = s1.ITEM.ITRATE
                    }
                    
                    ).ToList<OrderDetailsViewModel>()
                }

                ).ToList<OrderViewModel>();

            }

            if (orders.Count == 0)
            {
                return NotFound();
            }

            return Ok(orders);

        }

        [Route("api/Order/{pid}")]
        // GET api/Order/P001  
        [ResponseType(typeof(OrderViewModel))]
        public IHttpActionResult GetOrderByID(string pid)
        {
            IList<OrderViewModel> orders = null;

            db.POMASTERs.Include(c => c.SUPPLIER);
            db.PODETAILs.Include(c => c.ITEM);
            db.POMASTERs.Include(db => db.PODETAILs);

            using (var ctx = new FSDEntities())
            {
                orders = ctx.POMASTERs.Where(s => s.PONO == pid).Select(s => new OrderViewModel()
                {
                    PoNo = s.PONO,
                    PoDate = s.PODATE,
                    SupLNo = s.SUPLNO,
                    SuplName = s.SUPPLIER.SUPLNAME,
                    SuplAddr = s.SUPPLIER.SUPLADDR,
                    OrderDetails = ctx.PODETAILs.Where(s1 => s1.PONO == s.PONO).Select(s1 => new OrderDetailsViewModel()
                    {
                        ItCode = s1.ITCODE,
                        CPoNo = s1.PONO,
                        ItDesc = s1.ITEM.ITDESC,
                        Qty = s1.QTY,
                        ItRate = s1.ITEM.ITRATE
                    }

                    ).ToList<OrderDetailsViewModel>()
                }

                ).ToList<OrderViewModel>();

            }

            orders.Where(s => s.PoNo == pid);
            if (orders.Count == 0)
            {
                return NotFound();
            }

            return Ok(orders);

        }


   
        public IHttpActionResult PostNewOrder(OrderViewModel order)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

            string supNo=null;
            string itNo = null;
            string PoNo = null;


            using (var ctx = new FSDEntities())
            {

                var existingSupplier = ctx.SUPPLIERs.Where(s => s.SUPLNAME.Equals(order.SuplName)).FirstOrDefault<SUPPLIER>();


                if (existingSupplier != null)
                {
                    supNo = existingSupplier.SUPLNO;
                    existingSupplier.SUPLADDR = order.SuplAddr;
                }
                else
                {

                    var SupNumberList = from x in ctx.SUPPLIERs
                                        select new
                                        {
                                            sublstring = x.SUPLNO.Substring(1)
                                        };

                    var MaxSupNum = SupNumberList.Max(c => c.sublstring);

                    var supMaxNo = Convert.ToInt32(MaxSupNum) + 1;
                    supNo = "S" + supMaxNo.ToString("D3");

                    ctx.SUPPLIERs.Add(new SUPPLIER()
                    {
                        SUPLNO = supNo,
                        SUPLNAME = order.SuplName,
                        SUPLADDR = order.SuplAddr
                    });
                }
                ctx.SaveChanges();

            }



            using (var ctx = new FSDEntities())
            {

                var PoNumberList = from x in ctx.POMASTERs
                                   select new
                                   {
                                       Postring = x.PONO.Substring(1)
                                   };

                var MaxPoNum = PoNumberList.Max(c => c.Postring);

                var PoMaxNo = Convert.ToInt32(MaxPoNum) + 1;
                 PoNo = "P" + PoMaxNo.ToString("D3");

                    ctx.POMASTERs.Add(new POMASTER()
                    {
                        PONO = PoNo,
                        PODATE = order.PoDate,
                        SUPLNO = supNo
                    });
                
                ctx.SaveChanges();
            }




            using (var ctx = new FSDEntities())
            {
                foreach (OrderDetailsViewModel item in order.OrderDetails)
                {

                    var existingItem = ctx.ITEMs.Where(s => s.ITDESC.Equals(item.ItDesc)).FirstOrDefault<ITEM>();
                    if (existingItem != null)
                    {
                        itNo = existingItem.ITCODE;
                        existingItem.ITRATE = item.ItRate;
                    }
                    else
                    {


                        var ItemNumberList = from x in ctx.ITEMs
                                             select new
                                             {
                                                 itemstring = x.ITCODE.Substring(1)
                                             };

                        var MaxItemNum = ItemNumberList.Max(c => c.itemstring);

                        var itMaxNo = Convert.ToInt32(MaxItemNum) + 1;
                        itNo = "I" + itMaxNo.ToString("D3");

                        ctx.ITEMs.Add(new ITEM()
                        {
                            ITCODE = itNo,
                            ITDESC = item.ItDesc,
                            ITRATE = item.ItRate
                        });
                    }


                    ctx.PODETAILs.Add(new PODETAIL()
                    {
                        PONO = PoNo,
                        ITCODE = itNo,
                        QTY = item.Qty
                    });


                    ctx.SaveChanges();
                }
            }

            return Ok();
        }




        [Route("api/Order/Put")]
       public IHttpActionResult Put(OrderViewModel order)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

          
            using (var ctx = new FSDEntities())
            {
                var existingPo = ctx.POMASTERs.Where(s => s.PONO.Equals(order.PoNo)).FirstOrDefault<POMASTER>();


                if (existingPo != null)
                {
                    existingPo.PODATE = order.PoDate;
                    ctx.SaveChanges();
                }
                else
                {
                    return NotFound();
                }

                
                foreach (OrderDetailsViewModel item in order.OrderDetails)
                {

                    var existingItem = ctx.ITEMs.Where(s => s.ITCODE.Equals(item.ItCode)).FirstOrDefault<ITEM>();
                    if (existingItem != null)
                    {
                        existingItem.ITRATE = item.ItRate;
                    }

                    var existingChildPo = ctx.PODETAILs.Where(s => s.PONO.Equals(item.CPoNo)).FirstOrDefault<PODETAIL>();

                    if (existingChildPo != null)
                    {
                        existingChildPo.QTY = item.Qty;
                    }
                    ctx.SaveChanges();

                }


            }

            return Ok();
        }



        [Route("api/Order/Delete/{pid}")]
       
        public IHttpActionResult Delete(string Pid)
        {
            using (var ctx = new FSDEntities())
            {
                if (Pid.Length <= 0)
                    return BadRequest("Not a valid PO Order id");

                var orderdetails = ctx.PODETAILs.Where(s => s.PONO.Equals(Pid)).FirstOrDefault<PODETAIL>();
                if (orderdetails != null)
                {
                    ctx.Entry(orderdetails).State = System.Data.Entity.EntityState.Deleted;
                    ctx.SaveChanges();
                }
                var orderPo = ctx.POMASTERs.Where(s => s.PONO.Equals(Pid)).FirstOrDefault<POMASTER>();
                if (orderPo != null)
                {
                    ctx.Entry(orderPo).State = System.Data.Entity.EntityState.Deleted;
                    ctx.SaveChanges();
                }
            }
            return Ok();

        }

        }
}
