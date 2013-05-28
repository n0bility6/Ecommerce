﻿using System.Collections.Generic;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Ecommerce.Entities;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Pages;
using NHibernate;
using NHibernate.Criterion;
using MrCMS.Models;

namespace MrCMS.Web.Apps.Ecommerce.Services.Products
{
    public class ProductOptionManager : IProductOptionManager
    {
        private readonly ISession _session;

        public ProductOptionManager(ISession session)
        {
            _session = session;
        }

        public IList<ProductSpecificationAttribute> ListSpecificationAttributes()
        {
            return _session.QueryOver<ProductSpecificationAttribute>().Cacheable().List();
        }

        public ProductSpecificationAttribute GetSpecificationAttribute(int id)
        {
            return _session.QueryOver<ProductSpecificationAttribute>().Where(x => x.Id == id).Cacheable().SingleOrDefault();
        }

        public void AddSpecificationAttribute(ProductSpecificationAttribute option)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.Name))
                return;
            if (AnyExistingAtrributesWithName(option))
                return;
            _session.Transact(session => session.Save(option));
        }

        public void UpdateSpecificationAttribute(ProductSpecificationAttribute option)
        {
            if (AnyExistingAtrributesWithName(option))
                return;
            _session.Transact(session => session.Update(option));
        }

        public void DeleteSpecificationAttribute(ProductSpecificationAttribute option)
        {
            _session.Transact(session => session.Delete(option));
        }


        public IList<ProductSpecificationAttributeOption> ListSpecificationAttributeOptions(int id)
        {
            return _session.QueryOver<ProductSpecificationAttributeOption>().Where(x=>x.ProductSpecificationAttribute.Id==id).Cacheable().List();
        }

        public void AddSpecificationAttributeOption(ProductSpecificationAttributeOption option)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.Name))
                return;
            if (AnyExistingAtrributeOptionsWithName(option))
                return;
            option.ProductSpecificationAttribute.Options.Add(option);
            _session.Transact(session => session.Save(option));
        }

        public void UpdateSpecificationAttributeOption(ProductSpecificationAttributeOption option)
        {
            if (AnyExistingAtrributeOptionsWithName(option))
                return;
            _session.Transact(session => session.Update(option));
        }

        public void UpdateSpecificationAttributeOptionDisplayOrder(IList<SortItem> options)
        {
            _session.Transact(session => options.ForEach(item =>
            {
                var formItem = session.Get<ProductSpecificationAttributeOption>(item.Id);
                formItem.DisplayOrder = item.Order;
                session.Update(formItem);
            }));
        }

        public void DeleteSpecificationAttributeOption(ProductSpecificationAttributeOption option)
        {
            _session.Transact(session => session.Delete(option));
        }

        private bool AnyExistingAtrributesWithName(ProductSpecificationAttribute option)
        {
            return _session.QueryOver<ProductSpecificationAttribute>()
                           .Where(
                               specificationOption =>
                               specificationOption.Name.IsInsensitiveLike(option.Name, MatchMode.Exact))
                           .RowCount() > 0;
        }
        private bool AnyExistingAtrributeOptionsWithName(ProductSpecificationAttributeOption option)
        {
            return _session.QueryOver<ProductSpecificationAttributeOption>()
                           .Where(
                               specificationOption =>
                               specificationOption.Name.IsInsensitiveLike(option.Name, MatchMode.Exact) && specificationOption.ProductSpecificationAttribute.Id==option.ProductSpecificationAttribute.Id)
                           .RowCount() > 0;
        }
        private bool AnyExistingOptionsWithName(ProductAttributeOption option)
        {
            return _session.QueryOver<ProductAttributeOption>()
                           .Where(
                               specificationOption =>
                               specificationOption.Name.IsInsensitiveLike(option.Name, MatchMode.Exact))
                           .RowCount() > 0;
        }

        public void AddAttributeOption(ProductAttributeOption productAttributeOption)
        {
            if (string.IsNullOrWhiteSpace(productAttributeOption.Name))
                return;
            if (!AnyExistingOptionsWithName(productAttributeOption))
                _session.Transact(session => session.Save(productAttributeOption));
        }

        public void UpdateAttributeOption(ProductAttributeOption option)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.Name))
                return;
            if (AnyExistingOptionsWithName(option))
                return;
            _session.Transact(session => session.Update(option));
        }

        public IList<ProductAttributeOption> ListAttributeOptions()
        {
            return _session.QueryOver<ProductAttributeOption>().Cacheable().List();
        }

        public void DeleteAttributeOption(ProductAttributeOption option)
        {
            _session.Transact(session => session.Delete(option));
        }

        public void SetSpecificationValue(Product product, string optionName, string value)
        {
            var specificationOption = _session.QueryOver<ProductSpecificationAttribute>().Where(option => option.Name == optionName).Take(1).SingleOrDefault();
            if (specificationOption == null)
                return;
            var values = _session.QueryOver<ProductSpecificationValue>().Where(specificationValue => specificationValue.Option == specificationOption && specificationValue.Product == product).Cacheable().List();
            if (values.Any())
            {
                var specificationValue = values.First();
                specificationValue.Value = value;
                _session.Transact(session => session.Update(specificationValue));
            }
            else
            {
                _session.Transact(session => session.Save(new ProductSpecificationValue
                {
                    Product = product,
                    Option = specificationOption,
                    Value = value
                }));
            }
        }

        public void SetAttributeValue(ProductVariant productVariant, string attributeName, string value)
        {
            var specificationOption = _session.QueryOver<ProductAttributeOption>().Where(option => option.Name == attributeName).Take(1).SingleOrDefault();
            if (specificationOption == null)
                return;
            var values =
                _session.QueryOver<ProductAttributeValue>()
                        .Where(
                            specificationValue => specificationValue.Option == specificationOption &&
                                                  specificationValue.ProductVariant == productVariant)
                        .Cacheable()
                        .List();
            if (values.Any())
            {
                var specificationValue = values.First();
                specificationValue.Value = value;
                _session.Transact(session => session.Update(specificationValue));
            }
            else
            {
                _session.Transact(session => session.Save(new ProductAttributeValue
                {
                    ProductVariant = productVariant,
                    Option = specificationOption,
                    Value = value
                }));
            }
        }
    }
}