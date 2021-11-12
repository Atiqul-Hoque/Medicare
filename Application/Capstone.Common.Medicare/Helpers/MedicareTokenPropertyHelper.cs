using Capstone.Common.Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Helpers
{
    public static class MedicareTokenPropertyHelper
    {
        
        
        public static void updateProperty(MedicareTokenProperty property)
        {
            using (TenantDBContext ctx = new TenantDBContext())
            {
                ctx.Update(property);
                ctx.SaveChanges();
            }

        }
        public static void createProperty(MedicareTokenProperty property)
        {
            

            using (TenantDBContext ctx = new TenantDBContext())
            {
                ctx.Add(property);
                ctx.SaveChanges();
                ctx.Entry<MedicareTokenProperty>(property).State = EntityState.Detached;
            }
           
        }
      
    }
}
