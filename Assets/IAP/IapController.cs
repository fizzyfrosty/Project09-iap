using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JFoundation
{
    public interface IapControllerDelegate
    {
        void DidValidateProducts(List<StoreProduct> products, bool isSuccessful, string errorMessage);
        void DidBuyProduct(StoreProduct product, bool isSuccessful, string errorMessage);
        void DidRestoreProduct(StoreProduct product, bool isSuccessful, string errorMessage);
    }

    public class IapController : MonoBehaviour
    {

        Debugger debugger;
        IapControllerDelegate iapDelegate;

        // May need to remove/deprecate
        struct Product
        {
            ShopProductNames shopName;
            string name;
            int value;
            bool isActive;
        }

        Dictionary<string, Product> products;

        void Awake()
        {
            debugger = DependencyLoader.LoadGameObject<Debugger>("Debugger", this, gameObject) as Debugger;
            IAPManager.Instance.InitializeIAPManager(InitializeResultCallback);
        }

        public void SetDependencies(IapControllerDelegate iapDelegate)
        {
            this.iapDelegate = iapDelegate;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        // ----------------- Public Methods ------------------

        public void BuyProduct(string productName)
        {
            if (products.ContainsKey(productName))
            {
                ShopProductNames shopProductName = IAPManager.Instance.ConvertNameToShopProduct(productName);
                IAPManager.Instance.BuyProduct(shopProductName, ProductBoughtCallback);
                debugger.Log("Attempting to buy a product: " + productName);
            }
        }

        public void RestoreAllPurchases()
        {
            IAPManager.Instance.RestorePurchases(ProductRestoredCallback);
        }

        // ----------------- End of Public Methods ------------------

        // ----------------- Private Helper Methods -----------------------

        private void InitializeResultCallback(IAPOperationStatus status, string message, List<StoreProduct> shopProducts)
        {
            if (status == IAPOperationStatus.Success)
            {
                debugger.Log("IAP Successfully validated products.");
                //IAP was successfully initialized
                NotifyDelegateDidValidateProducts(shopProducts, true, message);
            }
            else
            {
                debugger.Log("IAP Error validating products: "+ message);
                NotifyDelegateDidValidateProducts(shopProducts, false, message);
            }
        }

        // automatically called after one product is bought
        // this is an example of product bought callback
        private void ProductBoughtCallback(IAPOperationStatus status, string message, StoreProduct product)
        {
            if (status == IAPOperationStatus.Success)
            {
                debugger.Log("IAP Successfully purchased product: " + product.productName);

                // Notify delegate
                NotifyDelegateDidBuyProduct(product, true, message);
            }
            else
            {
                //an error occurred in the buy process, log the message for more details
                debugger.Log("IAP Buy product failed: " + message);

                // Notify delegate
                NotifyDelegateDidBuyProduct(product, false, message);
            }
        }

        private void ProductRestoredCallback(IAPOperationStatus status, string message, StoreProduct product)
        {
            if (status == IAPOperationStatus.Success)
            {

                debugger.Log("IAP Successfully restored product: " + product.productName);
                NotifyDelegateDidRestorePurchase(product, true, message);
            }
            else
            {
                //an error occurred in the buy process, log the message for more details
                debugger.Log("IAP Failed to restore product: " + message);

                NotifyDelegateDidRestorePurchase(product, false, message);
            }
        }

        // ----------------- End of Private Helper Methods -----------------------

        // ---------------- Delegate Notification Callbacks --------------------

        void NotifyDelegateDidRestorePurchase(StoreProduct product, bool isSuccessful, string errorMessage)
        {
            if (DependencyLoader.DependencyCheck<IapControllerDelegate>(iapDelegate, this, gameObject, debugger))
            {
                iapDelegate.DidRestoreProduct(product, isSuccessful, errorMessage);
            }
        }

        void NotifyDelegateDidBuyProduct(StoreProduct product, bool isSuccessful, string errorMessage)
        {
            if (DependencyLoader.DependencyCheck<IapControllerDelegate>(iapDelegate, this, gameObject, debugger))
            {
                iapDelegate.DidBuyProduct(product, isSuccessful, errorMessage);
            }
        }

        void NotifyDelegateDidValidateProducts(List<StoreProduct> products, bool isSuccessful, string errorMessage)
        {
            if (DependencyLoader.DependencyCheck<IapControllerDelegate>(iapDelegate, this, gameObject, debugger))
            {
                iapDelegate.DidValidateProducts(products, isSuccessful, errorMessage);
            }
        }

        // ---------------- End of Delegate Notification Callbacks --------------------
    }
}
