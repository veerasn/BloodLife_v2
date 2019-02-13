using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BloodLife.Repository;
using BloodLife.Filters;
using BloodLife.Utility;
using BloodLife.ViewModels;
using BloodLife.Models;
using System.Data;
using System.Data.SqlClient;

namespace BloodLife.Controllers
{
    public class ShoppingController : Controller
    {
        #region Other Class references ...         
        // Instance on Unit of Work         
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        private int _memberId;
        public int memberId
        {
            get { return Convert.ToInt32(Session["MemberId"]); }
            set { _memberId = Convert.ToInt32(Session["MemberId"]); }
        }
        #endregion


        // GET: Shopping
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Add Product To Cart
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ActionResult AddProductToCart(string productId)
        {
            //Adding to cart 
            Cart c = new Cart();
            c.DateCreated = DateTime.Now;
            c.CheckedOut = false;
            c.UserID = memberId;
            c.Urgency = 0;
            c.Location = "A";
            _unitOfWork.GetRepositoryInstance<Cart>().Add(c);

            //Adding to cartitem
            CartItem i = new CartItem();
            i.CartID = c.CartID;
            i.ProductID = productId;
            i.Quantity = 1;
            i.Price = 0;
            i.Indication = "A";
            i.Alert = "A";
            i.Notice = "A";
            _unitOfWork.GetRepositoryInstance<CartItem>().Add(i);
            _unitOfWork.SaveChanges();

            TempData["ProductAddedToCart"] = "Product added to cart successfully";
            return RedirectToAction("Index", "Search");
        }

        /// <summary>
        /// MyCart
        /// </summary>
        /// <returns>List of cart items</returns>
        public ActionResult MyCart()
        {
            List<USP_MemberShoppingCartDetails_Result> cd = _unitOfWork.GetRepositoryInstance<USP_MemberShoppingCartDetails_Result>().GetResultBySqlProcedure("USP_MemberShoppingCartDetails @memberId",
                new SqlParameter("memberId", System.Data.SqlDbType.Int) { Value = memberId }).ToList();
            return View(cd);
        }

        /// <summary>
        /// Remove Cart Item
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ActionResult RemoveCartItem(int productId)
        {
            Cart c = _unitOfWork.GetRepositoryInstance<Cart>().GetFirstOrDefaultByParameter(i => i.UserID == memberId && i.CheckedOut == false);
            CartItem ci = _unitOfWork.GetRepositoryInstance<CartItem>().GetFirstOrDefaultByParameter(i => i.CartID == c.CartID);
            ci.Quantity = 0;
            _unitOfWork.GetRepositoryInstance<CartItem>().Update(ci);
            _unitOfWork.SaveChanges();
            return RedirectToAction("MyCart");
        }
    }
}