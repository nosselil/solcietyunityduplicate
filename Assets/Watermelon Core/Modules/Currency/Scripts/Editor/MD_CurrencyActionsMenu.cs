using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    public static class CurrencyActionsMenu
    {
        [MenuItem("Actions/Currency/Get 200K Money", priority = 21)]
        private static void GetCoins()
        {
            CurrencyController.Set(CurrencyType.Money, 200000);
        }

        [MenuItem("Actions/Currency/Get 200K Money", true)]
        private static bool GetCoinsValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("Actions/Currency/No Money", priority = 21)]
        private static void NoMoney()
        {
            CurrencyController.Set(CurrencyType.Money, 0);
        }

        [MenuItem("Actions/Currency/No Money", true)]
        private static bool NoMoneyValidation()
        {
            return Application.isPlaying;
        }
    }
}