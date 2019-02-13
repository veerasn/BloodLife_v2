using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BloodLife.Models;
using BloodLife.ViewModels;
using BloodLife.Utility;
using BloodLife.Repository;
using BloodLife.Filters;

namespace BloodLife.Controllers
{
    public class AccountController : Controller
    {
        #region Other class references...
        // Instance on Unit of Work
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        #endregion

        #region Member Registration ...         
        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.UserType = 2;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Register(RegisterViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Adding Member
                Member mem = new Member();
                mem.FirstName = model.FirstName;
                mem.EmailId = model.UserEmailId;
                mem.Password = EncryptDecrypt.Encrypt(model.Password, true);
                mem.CreatedOn = DateTime.Now;
                mem.ModifiedOn = DateTime.Now;
                mem.IsActive = true;
                mem.IsDelete = false;
                _unitOfWork.GetRepositoryInstance<Member>().Add(mem);

                // Adding Member Role
                MemberRole mem_Role = new MemberRole();
                mem_Role.MemberId = mem.MemberId;
                mem_Role.RoleId = 2;
                _unitOfWork.GetRepositoryInstance<MemberRole>().Add(mem_Role);
                TempData["VerificationLinlMsg"] = "You are registered successfully.";
                Session["MemberId"] = mem.MemberId;

                Response.Cookies["MemberName"].Value = mem.FirstName;
                Response.Cookies["MemberRole"].Value = "User";

                return RedirectToAction("Index", "Home");
            }
            return View("Register", model);
        }

        public JsonResult CheckEmailExist(string UserEmailId)
        {
            int LoginMemberId = Convert.ToInt32(Session["MemberId"]);
            var EmailExist = _unitOfWork.GetRepositoryInstance<Member>().GetFirstOrDefaultByParameter(i => i.MemberId != LoginMemberId && i.EmailId == UserEmailId && i.IsDelete == false);
            return EmailExist == null ? Json(true, JsonRequestBehavior.AllowGet) : Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Member Login ...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string EncryptedPassword = EncryptDecrypt.Encrypt(model.Password, true);
                var user = _unitOfWork.GetRepositoryInstance<Member>().GetFirstOrDefaultByParameter(i => i.EmailId == model.UserEmailId && i.Password == EncryptedPassword && i.IsDelete == false);
                if (user != null && user.IsActive == true)
                {
                    Session["MemberId"] = user.MemberId;
                    Response.Cookies["MemberName"].Value = user.FirstName;
                    var roles = _unitOfWork.GetRepositoryInstance<MemberRole>().GetFirstOrDefaultByParameter(i => i.MemberId == user.MemberId);

                    if (roles != null && roles.RoleId != 3)
                    {
                        Response.Cookies["MemberRole"].Value = _unitOfWork.GetRepositoryInstance<Role>().GetFirstOrDefaultByParameter(i => i.RoleId == roles.RoleId).RoleName;
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Invalid username or password");
                        return View(model);
                    }

                    if (model.RememberMe)
                    {
                        Response.Cookies["RememberMe_UserEmailId"].Value = model.UserEmailId;
                        Response.Cookies["RememberMe_Password"].Value = model.Password;
                    }
                    else
                    {
                        Response.Cookies["RememberMe_UserEmailId"].Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies["RememberMe_Password"].Expires = DateTime.Now.AddDays(-1);
                    }

                    ViewBag.redirectUrl = (!string.IsNullOrEmpty(returnUrl) ? HttpUtility.HtmlDecode(returnUrl) : "/");
                }
                else
                {
                    if (user != null && user.IsActive == false)
                        ModelState.AddModelError("Password", "Your account in not verified");
                    else
                        ModelState.AddModelError("Password", "Invalid username or password");
                }
            }
            return PartialView("_Login", model);
        }
        #endregion
    }
}