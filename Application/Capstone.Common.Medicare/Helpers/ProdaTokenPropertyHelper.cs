using Capstone.Common.Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Helpers
{
    public static class ProdaTokenPropertyHelper
    {
       
        public static void updateProdaTokenProperty(ProdaTokenProperty prodaTokenProperty)
        {
            using (TenantDBContext ctx = new TenantDBContext())
            {
                prodaTokenProperty.Modified = DateTime.UtcNow;
                ctx.Update(prodaTokenProperty);
                ctx.SaveChanges();
            }

        }
        public static int CreateProdaTokenProperty(ProdaTokenProperty prodaTokenProperty)
        {
            try
            {
                using (TenantDBContext ctx = new TenantDBContext())
                {

                    ctx.ProdaTokenProperties.Add(prodaTokenProperty);
                    int iRows = ctx.SaveChanges();
                    ctx.Entry<ProdaTokenProperty>(prodaTokenProperty).State = EntityState.Detached;
                    return iRows;
                }

            }
            catch(Exception e)
            {
                
                return 0;
            }
           
        }
       

    }
}
