using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public string URL { get; set; }
        public string requestId { get; set; }
        public Boolean transaccionOk { get; set; }
        public string estadotransaccion { get; set; }
        public string motivotransaccion { get; set; }
        public string Autorizacion { get; set; }
        public string referencia { get; set; }
        public string fecha { get; set; }
        
    }
}