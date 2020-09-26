using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JFoundation;

public class IapGameController : MonoBehaviour, IapControllerDelegate
{

    IapController iapController;
    Debugger debugger;
    public List<StoreProduct> products;

    public event Action<StoreProduct, bool, string> RestoredProductEvent;
    public event Action<StoreProduct, bool, string> BoughtProductEvent;
    public event Action<List<StoreProduct>, bool, string> ValidatedProductsEvent;

    void Awake()
    {
        iapController = gameObject.AddComponent<IapController>() as IapController;
        iapController.SetDependencies(this);

        debugger = DependencyLoader.LoadGameObject<Debugger>("Debugger", this, gameObject) as Debugger;

        if (this.products == null)
        {
            this.products = new List<StoreProduct>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ValidateProducts();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // --------------- Public Methods -------------

    public void BuyAProduct()
    {
        if (HasProducts())
        {
            StoreProduct product = products[0];
            string productName = product.productName;
            iapController.BuyProduct(productName);
        }
    }

    // Retrieve products from online first time
    public void ValidateProducts()
    {
        iapController.ValidateProducts();
    }

    public void RestoreAllPurchases()
    {
        iapController.RestoreAllPurchases();
    }

    // --------------- End of Public Methods -------------

    // ---------------- Private Helper Methods ---------------

    bool HasProducts()
    {
        bool hasProducts = false;

        if (products.Count > 0)
        {
            hasProducts = true;
        }

        return hasProducts;
    }

    // ---------------- End of Private Helper Methods ---------------

    // ------------ IAP Controller Delegate Callbacks ------------

    void IapControllerDelegate.DidValidateProducts(List<StoreProduct> products, bool isSuccessful, string errorMessage)
    {
        // Should be called automatically on startup

        this.products = products;

        string firstProduct = string.Format("First product is:{0} for {1}", products[0].productName, products[0].localizedPriceString);
        debugger.Log(firstProduct);

        if (ValidatedProductsEvent != null)
        {
            ValidatedProductsEvent(this.products, isSuccessful, errorMessage);
        }
    }

    void IapControllerDelegate.DidBuyProduct(StoreProduct product, bool isSuccessful, string errorMessage)
    {
        debugger.Log("Bought a product!", this, gameObject);

        if (BoughtProductEvent != null)
        {
            BoughtProductEvent(product, isSuccessful, errorMessage);
        }
    }

    void IapControllerDelegate.DidRestoreProduct(StoreProduct product, bool isSuccessful, string errorMessage)
    {
        // Products must be initially populated (validated) before it can be restored.
        if (HasProducts())
        {
            for (int i = 0; i < products.Count; i++)
            {
                StoreProduct currentProduct = products[i];

                if (currentProduct.productName == product.productName)
                {
                    // Replace it
                    products[i] = product;

                    debugger.Log("Restored a purchase: " + product.productName, this, gameObject);
                    break;
                }
            }
        }
        else
        {
            // If it does not exist, add it
            products.Add(product);

            debugger.Log("Restored a purchase that was not validated: " + product.productName, this, gameObject);
        }

        if (RestoredProductEvent != null)
        {
            RestoredProductEvent(product, isSuccessful, errorMessage);
        }
    }

    // ------------ End of IAP Controller Delegate Callbacks ------------
}
