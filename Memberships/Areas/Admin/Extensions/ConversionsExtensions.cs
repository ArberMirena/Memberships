﻿using Memberships.Areas.Admin.Models;
using Memberships.Entities;
using Memberships.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace Memberships.Areas.Admin.Extensions
{
    public static class ConversionsExtensions
    {
        #region Product

        public static async Task<IEnumerable<ProductModel>> Convert(this IEnumerable<Product> products, ApplicationDbContext db)
        {
            if (products.Count().Equals(0)) return new List<ProductModel>();
            var texts = await db.ProductLinkTexts.ToListAsync();
            var types = await db.ProductTypes.ToListAsync();

            return from p in products
                   select new ProductModel
                   {
                       Id = p.Id,
                       Title = p.Title,
                       Description = p.Description,
                       Url = p.Url,
                       ProductLinkTextId = p.ProductLinkTextId,
                       ProductTypeId = p.ProductTypeId,
                       ProductLinkTexts = texts,
                       ProductTypes = types,

                   };
        }

        public static async Task<IEnumerable<ProductItemModel>> Convert(this IQueryable<ProductItem> productItems, ApplicationDbContext db)
        {
            if (productItems.Count().Equals(0)) return new List<ProductItemModel>();

            return await (from pi in productItems
                          select new ProductItemModel
                          {
                              ItemId = pi.ItemId,

                              ProductId = pi.ProductId,

                              ProductTitle = db.Products.FirstOrDefault(
                                  p => p.Id.Equals(pi.ProductId)).Title,

                              ItemTitle = db.Items.FirstOrDefault(
                                  i=> i.Id.Equals(pi.ItemId)).Title,

                              


                          }).ToListAsync();
        }

        #endregion

        #region ProductItem
        public static async Task<ProductModel> Convert(this Product product, ApplicationDbContext db)
        {

            var text = await db.ProductLinkTexts.FirstOrDefaultAsync (p => p.Id.Equals(product.ProductLinkTextId));
               
            var type = await db.ProductTypes.FirstOrDefaultAsync(p => p.Id.Equals(product.ProductTypeId));

            var model =   new ProductModel
                   {
                       Id = product.Id,
                       Title = product.Title,
                       Description = product.Description,
                       Url = product.Url,
                       ProductLinkTextId = product.ProductLinkTextId,
                       ProductTypeId = product.ProductTypeId,
                       ProductLinkTexts = new List<ProductLinkText>(),
                       ProductTypes = new List<ProductType>(),

                   };

            model.ProductLinkTexts.Add(text);
            model.ProductTypes.Add(type);

            return model;
        }

        public static async Task<ProductItemModel> Convert(this ProductItem productItem, ApplicationDbContext db, bool addListData = true)
        {

            var model = new ProductItemModel
            {
                ItemId = productItem.ItemId,
                ProductId = productItem.ProductId,
                Items =  addListData ? await db.Items.ToListAsync() : null,
                Products = addListData ?  await db.Products.ToListAsync() : null,
                ItemTitle = (await db.Items.FirstOrDefaultAsync(i => i.Id.Equals(productItem.ItemId))).Title,
                ProductTitle = (await db.Products.FirstOrDefaultAsync(p => p.Id.Equals(productItem.ProductId))).Title,

            };

            return model;
        }

        public static async Task<bool> CanChange(this ProductItem productItem, ApplicationDbContext db)
        {
            var oldPI = await db.ProductItems.CountAsync(
                pi => pi.ProductId.Equals(productItem.OldProductId) &&
                pi.ItemId.Equals(productItem.OldItemId));

            var newPI = await db.ProductItems.CountAsync(
                pi => pi.ProductId.Equals(productItem.ProductId) &&
                pi.ItemId.Equals(productItem.ItemId));

            //oldPi.Equals(1) returns true if old value exists or viceversa 
            return oldPI.Equals(1) && newPI.Equals(0);
        }

        public static async Task Change(this ProductItem productItem, ApplicationDbContext db)
        {
            var oldProductItem = await db.ProductItems.FirstOrDefaultAsync(
                pi => pi.ProductId.Equals(productItem.OldProductId) &&
                pi.ItemId.Equals(productItem.OldItemId));

            var newProductItem = await db.ProductItems.FirstOrDefaultAsync(
                pi => pi.ProductId.Equals(productItem.ProductId) &&
                pi.ItemId.Equals(productItem.ItemId));

            if(oldProductItem != null && newProductItem == null)
            {
                newProductItem  = new ProductItem
                {
                    ItemId = productItem.ItemId,
                    ProductId = productItem.ProductId

                };

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try 
                    {
                        db.ProductItems.Remove(oldProductItem);
                        db.ProductItems.Add(newProductItem);

                        await db.SaveChangesAsync();
                        transaction.Complete();
                    }

                    catch { transaction.Dispose(); }
                }
            }
        }

        #endregion

        #region SubscriptionProduct

        public static async Task<IEnumerable<SubscriptionProductModel>> Convert(this IQueryable<SubscriptionProduct> subscriptionProducts, ApplicationDbContext db)
        {
            if (subscriptionProducts.Count().Equals(0)) return new List<SubscriptionProductModel>();

            return await (from pi in subscriptionProducts
                          select new SubscriptionProductModel
                          {
                              SubscriptionId = pi.SubscriptionId,

                              ProductId = pi.ProductId,

                              ProductTitle = db.Products.FirstOrDefault(
                                  p => p.Id.Equals(pi.ProductId)).Title,

                              SubscriptionTitle = db.Items.FirstOrDefault(
                                  i => i.Id.Equals(pi.SubscriptionId)).Title,




                          }).ToListAsync();
        }

        public static async Task<SubscriptionProductModel> Convert(this SubscriptionProduct subscriptionProducts, ApplicationDbContext db, bool addListData = true)
        {

            var model = new SubscriptionProductModel
            {
                SubscriptionId = subscriptionProducts.SubscriptionId,
                ProductId = subscriptionProducts.ProductId,
                Subscriptions = addListData ? await db.Subscriptions.ToListAsync() : null,
                Products = addListData ? await db.Products.ToListAsync() : null,
                SubscriptionTitle = (await db.Subscriptions.FirstOrDefaultAsync(s => s.Id.Equals(subscriptionProducts.SubscriptionId))).Title,
                ProductTitle = (await db.Products.FirstOrDefaultAsync(p => p.Id.Equals(subscriptionProducts.ProductId))).Title,

            };

            return model;
        }

        public static async Task<bool> CanChange(this SubscriptionProduct subscriptionProducts, ApplicationDbContext db)
        {
            var oldSP = await db.SubscriptionProducts.CountAsync(
                sp => sp.ProductId.Equals(subscriptionProducts.OldProductId) &&
                sp.SubscriptionId.Equals(subscriptionProducts.OldSubscriptionId));

            var newSP = await db.SubscriptionProducts.CountAsync(
                sp => sp.ProductId.Equals(subscriptionProducts.ProductId) &&
                sp.SubscriptionId.Equals(subscriptionProducts.SubscriptionId));

            //oldPi.Equals(1) returns true if old value exists or viceversa 
            return oldSP.Equals(1) && newSP.Equals(0);
        }

        public static async Task Change(this SubscriptionProduct subscriptionProducts, ApplicationDbContext db)
        {
            var oldSubscriptionProduct = await db.SubscriptionProducts.FirstOrDefaultAsync(
                sp => sp.ProductId.Equals(subscriptionProducts.OldProductId) &&
                sp.SubscriptionId.Equals(subscriptionProducts.OldSubscriptionId));

            var newSubscriptionProduct = await db.SubscriptionProducts.FirstOrDefaultAsync(
                sp => sp.ProductId.Equals(subscriptionProducts.ProductId) &&
                sp.SubscriptionId.Equals(subscriptionProducts.SubscriptionId));

            if (oldSubscriptionProduct != null && newSubscriptionProduct == null)
            {
                newSubscriptionProduct = new SubscriptionProduct
                {
                    SubscriptionId = subscriptionProducts.SubscriptionId,
                    ProductId = subscriptionProducts.ProductId

                };

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        db.SubscriptionProducts.Remove(oldSubscriptionProduct);
                        db.SubscriptionProducts.Add(newSubscriptionProduct);

                        await db.SaveChangesAsync();
                        transaction.Complete();
                    }

                    catch { transaction.Dispose(); }
                }
            }
        }

        #endregion
    }
}