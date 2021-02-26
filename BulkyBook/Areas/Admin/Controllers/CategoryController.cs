﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(int productPage = 1)
        {
            int PageSize = 2;
            CategoryVM categoryVM = new CategoryVM()
            {
                Categories = _unitOfWork.Category.GetAll()
            };

            var count = categoryVM.Categories.Count();
            categoryVM.Categories = categoryVM.Categories.OrderBy(p => p.Name)
                .Skip((productPage - 1) * PageSize).Take(2).ToList();

            categoryVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = "/Admin/Category/Index?productPage=:"
            };

            return View(categoryVM);
        }

        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if(id == null)
            {
                //create
                return View(category);
            }
            //edit
            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if(category.Id == 0)
                {
                    _unitOfWork.Category.Add(category);                    
                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Category.GetAll();
            return Json(new { data = allObj});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Category.Get(id);
            if(objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            else
            {
                _unitOfWork.Category.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["Success"] = "Category Successfully deleted";
                return Json(new { success = true, message = "Delete Successful" });
            }
        }

        #endregion
    }
}
