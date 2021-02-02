using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork; 

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {                        
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value)
                    .ToList().Count();

                //Default Int session for asp.net core
                HttpContext.Session.SetInt32(SD.ssShoppingCart, Count);
            }

            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            var productFromDB = _unitOfWork.Product.
                GetFirstOrDefault(u => u.Id == id,includeProperties: "Category,CoverType");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDB,
                ProductId = productFromDB.Id
            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //Add to Cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.
                    GetFirstOrDefault(u => u.ApplicationUserId == CartObject.ApplicationUserId &&
                     u.ProductId == CartObject.ProductId,includeProperties:"Product");

                if(cartFromDB == null)
                {
                    //no records exists
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDB.Count += CartObject.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDB);
                }
                _unitOfWork.Save();

                var Count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList().Count();
                //HttpContext.Session.SetObject(SD.ssShoppingCart, Count);  

                //Default Int session for asp.net core
                HttpContext.Session.SetInt32(SD.ssShoppingCart, Count);                

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDB = _unitOfWork.Product.
                GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,CoverType");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDB,
                    ProductId = productFromDB.Id
                };
                return View(cartObj);
            }            
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }        
    }
}
